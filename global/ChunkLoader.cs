using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vec2IList = System.Collections.Generic.List<Godot.Vector2I>;

public partial class ChunkLoader : Node
{
	GameManager gameManager;

	public WorldManager WorldRoot;

	public int RenderDistance = 3;
	private int RenderDistanceSquared;
	public Vector2I chunk_size = new Vector2I(16, 16);

	private FastNoiseLite NoiseLite;
	private float Frequency = 0.04f;
	private int Seed;
	private int NoiseType = (int)FastNoiseLite.NoiseTypeEnum.Perlin;
	private float TreeScattering = 4;
	private float TreeSpawningThreashold = 0f;

	private Vector3 PlayerPosition;
	private Vector2I PlayerChunkPosition;
	private Vector2I CachedPlayerChunkPosition;
	private readonly object ChunkLock = new object();

	public Dictionary<Vector2I, MeshInstance3D> LoadedChunkPlanes = new Dictionary<Vector2I, MeshInstance3D>();
	public Dictionary<Vector2I, TreeRes> LoadedObjects = new Dictionary<Vector2I, TreeRes>();

	private Vec2IList QueuedChunks = new();
	private Dictionary<Vector2I, Vector2I> QueuedTrees = new();

	private Vec2IList ChunksToUnload = new();

	public bool IsChunkLoading = false;

	private const int MAX_CHUNK_PLANE_RENDER_PER_FRAME = 50;
	private const int MAX_OBJECT_RENDER_PER_FRAME = 50;

	public override void _Ready()
	{
		gameManager = GetTree().Root.GetNode<GameManager>("GameManager");

		RenderDistanceSquared = RenderDistance * RenderDistance;

		SetUpNoise();
	}

	public override void _Process(double delta)
	{
		PlayerPosition = gameManager.Player.CameraGroundPivot.Position;
		Vector2I newPlayerChunkPos = PositionToChunkCoordinates(PlayerPosition);

		// Only update chunks when player moves to a new chunk
		if (newPlayerChunkPos != PlayerChunkPosition)
		{
			PlayerChunkPosition = newPlayerChunkPos;

			if (!IsChunkLoading)
			{
				IsChunkLoading = true;
				CachedPlayerChunkPosition = PlayerChunkPosition; // Cache for thread
				_ = AsyncManager().ContinueWith(_ => IsChunkLoading = false);
			}
		}

		ChunkRenderingManager();
	}

	private void SetUpNoise()
	{
		NoiseLite = new FastNoiseLite();
		NoiseLite.NoiseType = (FastNoiseLite.NoiseTypeEnum)NoiseType;
		NoiseLite.Frequency = Frequency;
		NoiseLite.Seed = Seed;
	}

	private async Task AsyncManager()
	{
		Vec2IList chunksToQueue = new();
		Vec2IList chunksToUnqueue = new();

		await Task.Run(() =>
		{
			// Use cached position to avoid race conditions
			Vector2I playerPos = CachedPlayerChunkPosition;

			// Find chunks to load
			for (int x = -RenderDistance; x <= RenderDistance; x++)
			{
				for (int y = -RenderDistance; y <= RenderDistance; y++)
				{
					if (IsInRadius(x, y))
					{
						Vector2I newChunk = new Vector2I(x + playerPos.X, y + playerPos.Y);
						chunksToQueue.Add(newChunk);
					}
				}
			}

			// Find chunks to unload
			lock (ChunkLock)
			{
				foreach (Vector2I loadedChunk in LoadedChunkPlanes.Keys)
				{
					if (playerPos.DistanceTo(loadedChunk) > RenderDistance)
					{
						chunksToUnqueue.Add(loadedChunk);
					}
				}
			}
		});

		// Apply changes on main thread
		lock (ChunkLock)
		{
			foreach (Vector2I chunk in chunksToQueue)
			{
				if (!LoadedChunkPlanes.ContainsKey(chunk) && !QueuedChunks.Contains(chunk))
				{
					QueuedChunks.Add(chunk);
					CreateTree(chunk);
				}
			}

			foreach (Vector2I chunk in chunksToUnqueue)
			{
				if (!ChunksToUnload.Contains(chunk))
				{
					ChunksToUnload.Add(chunk);
				}
			}
		}
	}

	private void ChunkRenderingManager()
	{
		switch ((int)(gameManager.Tick % 2))
		{
			case 0:
				CreateChunkMesh();
				ChunkUnloader();
				break;
			case 1:
				CreateTree();
				break;
		}
	}

	private void CreateChunkMesh()
	{
		int Rendered = 0;
		Vec2IList dupQueuedChunks;

		lock (ChunkLock)
		{
			dupQueuedChunks = new Vec2IList(QueuedChunks);
		}

		foreach (Vector2I chunk in dupQueuedChunks)
		{
			Rendered += 1;

			MeshInstance3D newChunkMesh = CreatePlaneMesh();
			WorldRoot.Grounds.AddChild(newChunkMesh);
			newChunkMesh.Visible = true;
			newChunkMesh.Position = (Vector3)ChunkCoordinatesToPosition(chunk, false);

			lock (ChunkLock)
			{
				LoadedChunkPlanes[chunk] = newChunkMesh;
				QueuedChunks.Remove(chunk);
			}

			if (Rendered > MAX_CHUNK_PLANE_RENDER_PER_FRAME)
			{
				return;
			}
		}
	}

	private MeshInstance3D CreatePlaneMesh()
	{
		MeshInstance3D chunk_mesh = new MeshInstance3D();
		PlaneMesh plane_mesh = new PlaneMesh();
		plane_mesh.Size = chunk_size;
		plane_mesh.Material = WorldRoot.Ground_Material;
		plane_mesh.CenterOffset = new Vector3(chunk_size.X / 2, 0, chunk_size.Y / 2);
		chunk_mesh.Mesh = plane_mesh;
		return chunk_mesh;
	}

	private void CreateTree()
	{
		int Rendered = 0;

		Vec2IList dupQueuedTrees = QueuedTrees.Keys.ToList();
		foreach (Vector2I treePosition in dupQueuedTrees)
		{
			Rendered += 1;

			TreeRes newTree = (TreeRes)WorldRoot.Object_Array[0].Instantiate();

			newTree.ChunkPosition = QueuedTrees[treePosition];

			WorldRoot.Resources.AddChild(newTree);

			newTree.Position = new Godot.Vector3(treePosition.X, 0, treePosition.Y);

			QueuedTrees.Remove(treePosition);
			LoadedObjects[treePosition] = newTree;

			if (Rendered > MAX_OBJECT_RENDER_PER_FRAME)
			{
				return;
			}
		}
	}

	private void ChunkUnloader()
	{
		Vec2IList chunksToRemove;

		lock (ChunkLock)
		{
			chunksToRemove = new Vec2IList(ChunksToUnload);
		}

		foreach (Vector2I chunk in chunksToRemove)
		{
			lock (ChunkLock)
			{
				if (LoadedChunkPlanes.ContainsKey(chunk))
				{
					LoadedChunkPlanes[chunk].Free();
					LoadedChunkPlanes.Remove(chunk);
				}
				ChunksToUnload.Remove(chunk);
			}
		}
	}

	private void CreateTree(Vector2I chunk)
	{
		Vector2I chunk_from = (Vector2I)ChunkCoordinatesToPosition(chunk, true);
		Vector2I chunk_to = chunk_from + new Vector2I(chunk_size.X, chunk_size.Y);

		for (int x = chunk_from.X; x <= chunk_to.X; x++)
		{
			for (int y = chunk_from.Y; y <= chunk_to.Y; y++)
			{
				Vector2I newPosition = new Vector2I(x, y);

				if (!LoadedObjects.ContainsKey(newPosition) && !QueuedTrees.ContainsKey(newPosition))
				{
					float noise_value = NoiseLite.GetNoise2D(x, y);
					if (TreeSpawningThreashold < noise_value)
					{
						float spawn_chance = Mathf.Pow((noise_value + 1.0f) / 2.0f, TreeScattering);
						if (GD.Randf() < spawn_chance)
						{
							QueuedTrees[newPosition] = chunk;
						}
					}
				}
			}
		}
	}

	public bool IsInRadius(int x, int y) => x * x + y * y <= RenderDistanceSquared;

	// Fixed coordinate conversion - use correct chunk dimensions
	public Vector2I PositionToChunkCoordinates(Vector3 pos) =>
		new Vector2I(Mathf.FloorToInt(pos.X / chunk_size.X), Mathf.FloorToInt(pos.Z / chunk_size.Y));

	public object ChunkCoordinatesToPosition(Vector2I chunk_pos, bool in_vector2 = false)
	{
		if (in_vector2)
			return new Vector2I(chunk_pos.X * chunk_size.X, chunk_pos.Y * chunk_size.Y);
		else
			return new Vector3(chunk_pos.X * chunk_size.X, 0, chunk_pos.Y * chunk_size.Y);
	}

	public bool CanUnloadChunk(Vector2I chunk_pos) => PlayerChunkPosition.DistanceTo(chunk_pos) > RenderDistance;
}