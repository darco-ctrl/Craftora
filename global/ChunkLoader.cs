using Godot;
using Godot.Collections;
using System;

public partial class ChunkLoader : Node
{
	GameManager GM;

	public partial class ResourcesQueueData : GodotObject
	{
		public Vector2I ChunkPos;
		public int ItemType;
		public ResourcesQueueData(Vector2I p, int it)
		{
			ChunkPos = p;
			ItemType = it;
		}
	}

	public WorldManager WorldManager;

	public int RenderDistance = 3;
	public int RenderDistanceSquared;
	public Vector2I chunk_size = new Vector2I(16, 16);

	public FastNoiseLite NoiseLite;
	public float Frequency = 0.04f;
	public int Seed;
	public int NoiseType = (int)FastNoiseLite.NoiseTypeEnum.Perlin;
	public float TreeScattering = 3;

	public Dictionary<Vector2I, MeshInstance3D> LoadedChunks = new();
	public Dictionary<Vector2I, TreeRes> LoadedObjects = new();

	public const int MAX_UNLOADS_PER_FRAME = 10;

	public Array<Vector2I> QueuedChunks = new();
	public Dictionary<Vector2I, ResourcesQueueData> QueuedObjects = new();
	public Array<Vector2I> ChunksToUnlaod = new();
	public Array<Vector2I> ObjectsToUnload = new();

	public enum ItemType { TREE, STONE }
	public enum ChunkLoadPhase { QUEUE_CHUNKS_PHASE, QUEUE_RES_PHASE, RENDER_QUEUED_OBJECTS_PHASE }

	public override void _Ready()
	{
		GM = (GameManager)GetTree().Root.GetNode("GameManager");

		RenderDistanceSquared = RenderDistance * RenderDistance;
		Seed = (int)(GD.Randi() & int.MaxValue);
	}

	public override void _Process(double delta)
	{
		ChunkUnloaderProcess();
	}

	public void SetupReosourcesNoise()
	{
		NoiseLite = new FastNoiseLite();
		NoiseLite.NoiseType = (FastNoiseLite.NoiseTypeEnum)NoiseType;
		NoiseLite.Frequency = Frequency;
		NoiseLite.Seed = Seed;
	}

	public Array<Vector2I> MakeNewChunkList()
	{
		Array<Vector2I> chunks_to_load = new Array<Vector2I>();
		Vector2I player_chunk_position = PositionToChunkCoordinates(GM.Player.CameraGroundPivot.Position);

		for (int x = -RenderDistance; x <= RenderDistance; x++)
		{
			for (int y = -RenderDistance; y <= RenderDistance; y++)
			{
				if (IsInRadius(x, y))
				{
					Vector2I new_chunk = new Vector2I(x + player_chunk_position.X, y + player_chunk_position.Y);
					if (!LoadedChunks.ContainsKey(new_chunk))
						chunks_to_load.Add(new_chunk);
				}
			}
		}
		return chunks_to_load;
	}

	public void ChunkUnloaderPrepare()
	{
		foreach (var chunk in LoadedChunks.Keys)
		{
			if (CanUnloadChunk(chunk))
			{
				var plane = LoadedChunks[chunk];
				if (GodotObject.IsInstanceValid(plane))
					ChunksToUnlaod.Add(chunk);
				else
					LoadedObjects.Remove(chunk);
			}
		}

		foreach (var object_pos in LoadedObjects.Keys)
		{
			TreeRes Tree = LoadedObjects[object_pos];
			if (GodotObject.IsInstanceValid(Tree))
			{
				if (!LoadedChunks.ContainsKey(Tree.ChunkPosition))
					ObjectsToUnload.Add(object_pos);
			}
			else
				LoadedObjects.Remove(object_pos);
		}
	}

	public void ChunkUnloaderProcess()
	{
		for (int i = 0; i < MAX_UNLOADS_PER_FRAME; i++)
		{
			if (ChunksToUnlaod.Count > 0)
			{
				Vector2I chunk = ChunksToUnlaod[^1];
				ChunksToUnlaod.RemoveAt(ChunksToUnlaod.Count - 1);
				if (LoadedChunks.ContainsKey(chunk))
				{
					LoadedChunks[chunk].Free();
					LoadedChunks.Remove(chunk);
				}
			}

			if (ObjectsToUnload.Count > 0)
			{
				Vector2I obj = ObjectsToUnload[^1];
				ObjectsToUnload.RemoveAt(ObjectsToUnload.Count - 1);
				if (LoadedObjects.ContainsKey(obj))
				{
					LoadedObjects[obj].Free();
					LoadedObjects.Remove(obj);
				}
			}
		}
	}

	public void CreateResourceQueue()
	{
		foreach (var chunk in QueuedChunks)
		{
			Vector2I chunk_from = (Vector2I)ChunkCoordinatesToPosition(chunk, true);
			Vector2I chunk_to = chunk_from + new Vector2I(chunk_size.X, chunk_size.Y);

			for (int x = (int)chunk_from.X; x < (int)chunk_to.X; x++)
			{
				for (int y = (int)chunk_from.Y; y < (int)chunk_to.Y; y++)
				{
					Vector2I new_position = new Vector2I(x, y);
					if (!LoadedObjects.ContainsKey(new_position))
					{
						float noise_value = NoiseLite.GetNoise2D(x, y);
						if (noise_value > 0.1f)
						{
							float spawn_chance = Mathf.Pow((noise_value + 1.0f) / 2.0f, TreeScattering);
							if (GD.Randf() < spawn_chance)
							{
								QueuedObjects[new_position] = new ResourcesQueueData(chunk, (int)ItemType.TREE);
							}
						}
					}
				}
			}
		}
	}

	public void RenderGroundMesh()
	{
		if (QueuedChunks.Count > 0)
		{
			foreach (var chunk in new Array<Vector2I>(QueuedChunks))
			{
				MeshInstance3D new_chunk_mesh = CreatePlaneMesh(chunk_size);
				WorldManager.Grounds.AddChild(new_chunk_mesh);
				new_chunk_mesh.Visible = true;
				new_chunk_mesh.Position = (Vector3)ChunkCoordinatesToPosition(chunk, false);
				LoadedChunks[chunk] = new_chunk_mesh;
				QueuedChunks.Remove(chunk);
			}
		}
	}

	public MeshInstance3D CreatePlaneMesh(Vector2 plane_size)
	{
		MeshInstance3D chunk_mesh = new MeshInstance3D();
		PlaneMesh plane_mesh = new PlaneMesh();
		plane_mesh.Size = plane_size;
		plane_mesh.Material = WorldManager.Ground_Material;
		plane_mesh.CenterOffset = new Vector3(plane_size.X / 2, 0, plane_size.Y / 2);
		chunk_mesh.Mesh = plane_mesh;
		return chunk_mesh;
	}

	public void TreeLoader()
	{
		foreach (var block_pos in new Array<Vector2I>(QueuedObjects.Keys))
		{
			ResourcesQueueData CurrentObject = QueuedObjects[block_pos];
			TreeRes Tree = (TreeRes)WorldManager.Object_Array[CurrentObject.ItemType].Instantiate();
			Tree.ChunkPosition = CurrentObject.ChunkPos;
			LoadedObjects[block_pos] = Tree;
			Tree.Position = new Vector3(block_pos.X, 0, block_pos.Y);
			Tree.Visible = true;
			WorldManager.Resources.AddChild(Tree);
			QueuedObjects.Remove(block_pos);
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

	public bool CanUnloadChunk(Vector2I chunk_pos) => PositionToChunkCoordinates(GM.Player.CameraGroundPivot.Position).DistanceTo(chunk_pos) > RenderDistance;
}