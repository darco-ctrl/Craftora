using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Vec2IList = System.Collections.Generic.List<Godot.Vector2I>;

public partial class ChunkLoader : Node
{
	GameManager gameManager;

	public WorldManager WorldManager;

	private int RenderDistance = 3;
	private int RenderDistanceSquared;
	private Vector2I chunk_size = new Vector2I(16, 16);

	private FastNoiseLite NoiseLite;
	private float Frequency = 0.04f;
	private int Seed;
	private int NoiseType = (int)FastNoiseLite.NoiseTypeEnum.Perlin;
	private float TreeScattering = 3;
	private float TreeSpawningThreashold = -1f;

	private Vector3 PlayerPosition;
	private Vector2I PlayerChunkPosition;

	public Dictionary<Vector2I, MeshInstance3D> LoadedChunksPlane = new Dictionary<Vector2I, MeshInstance3D>();
	public Dictionary<Vector2I, TreeRes> LoadedObjects = new Dictionary<Vector2I, TreeRes>();

	private Vec2IList QueuedChunks = new();
	private Vec2IList QueuedTrees = new();

	private Vec2IList ChunksToUnload = new();
	private Vec2IList TreesTounload = new();

	public bool IsChunkLoading = false;

	public override void _Ready()
	{
		gameManager = GetTree().Root.GetNode<GameManager>("GameManager");

		SetUpNoise();
	}

	public override void _Process(double delta)
	{
		PlayerPosition = (Vector3I)(gameManager.Player.CameraGroundPivot.Position);
		PlayerChunkPosition = PositionToChunkCoordinates(PlayerPosition);

		if (!IsChunkLoading)
		{
			IsChunkLoading = true;
			_ = AsyncManager().ContinueWith(_ => IsChunkLoading = false);
		}

	}


	private void SetUpNoise()
	{
		NoiseLite.NoiseType = (Godot.FastNoiseLite.NoiseTypeEnum)NoiseType;
		NoiseLite.Frequency = Frequency;
		NoiseLite.Seed = Seed;
	}

	private async Task AsyncManager()
	{
		await Task.Run(() =>
		{
			ChunkQueueListPrep();
		});
	}

	private void ChunkRenderingManager()
	{
		switch ((int)(gameManager.Tick % 2))
		{
			case 0:
				CreateChunkMesh();
				break;
			case 1:
				CreateTree();
				break;
		}
	}

	private void CreateChunkMesh()
	{

	}

	private void CreateTree()
	{

	}

	private void ChunkQueueListPrep()
	{
		Vec2IList chunkToLoad = new();

		for (int x = -RenderDistance; x <= RenderDistance; x++)
		{
			for (int y = -RenderDistance; y <= RenderDistance; y++)
			{
				if (IsInRadius(x, y))
				{
					Vector2I newChunk = new Vector2I(x + PlayerChunkPosition.X, y + PlayerChunkPosition.Y);

					if (!LoadedChunksPlane.ContainsKey(newChunk) && !QueuedChunks.Contains(newChunk))
					{
						chunkToLoad.Add(newChunk);
						CreateTree(newChunk);
					}
				}
			}
		}

		Dictionary<Vector2I, MeshInstance3D> _loadedChunks = LoadedChunksPlane.Duplicate();

		foreach (Vector2I chunk in chunkToLoad)
		{
			if (_loadedChunks.ContainsKey(chunk))
			{
				_loadedChunks.Remove(chunk);
			}
			else
			{
				QueuedChunks.Add(chunk);
			}
		}

		ChunksToUnload = _loadedChunks.Keys.ToList();

		// LEFT AT CHUNK LOADING QUEUE SYSTEM

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

				if (!LoadedObjects.ContainsKey(newPosition) && !QueuedTrees.Contains(newPosition))
				{
					float noise_value = NoiseLite.GetNoise2D(x, y);
					if (TreeSpawningThreashold < noise_value)
					{
						float spawn_chance = Mathf.Pow((noise_value + 1.0f) / 2.0f, TreeScattering);
						if (GD.Randf() < spawn_chance)
						{
							QueuedTrees.Add(newPosition);
						}
					}
				}
			}
		}
	}

	public bool IsInRadius(int x, int y) => x * x + y * y <= RenderDistanceSquared;

	public Vector2I PositionToChunkCoordinates(Vector3 pos) => new Vector2I(Mathf.FloorToInt(pos.X / chunk_size.Y), Mathf.FloorToInt(pos.Z / chunk_size.Y));

	public object ChunkCoordinatesToPosition(Vector2I chunk_pos, bool in_vector2 = false)
	{
		if (in_vector2)
			return new Vector2I(Mathf.FloorToInt(chunk_pos.X * chunk_size.Y), Mathf.FloorToInt(chunk_pos.Y * chunk_size.Y));
		else
			return new Vector3(Mathf.FloorToInt(chunk_pos.X * chunk_size.Y), 0, Mathf.FloorToInt(chunk_pos.Y * chunk_size.Y));
	}

	public bool CanUnloadChunk(Vector2I chunk_pos) => PositionToChunkCoordinates(gameManager.Player.CameraGroundPivot.Position).DistanceTo(chunk_pos) > RenderDistance;
}