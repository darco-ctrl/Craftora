using Godot;
using System;

public partial class WorldManager : Node3D
{
    GameManager gameManager;
    ChunkLoader chunkLoader;

    [Export] public Node3D Grounds;
    [Export] public Node3D Resources;
    [Export] public Node3D PlacedObjects;
    [Export] public Godot.Collections.Array<PackedScene> Object_Array;

    [Export] public StandardMaterial3D Ground_Material;

    public override void _Ready()
    {
        gameManager = (GameManager)GetTree().Root.GetNode("GameManager");
        chunkLoader = (ChunkLoader)GetTree().Root.GetNode("ChunkLoader");

        chunkLoader.WorldRoot = this;
    }

    public override void _Process(double _delta)
    {
        if (Input.IsActionJustPressed("exclamation"))
        {
            //Debbug_Print();
        }
    }

    public void Debbug_Print()
    {
        GD.Print(
            "------------- CURRENTLY LOADED OBJECTS -------------", "\n",
            "StaticBody3D     - ", chunkLoader.LoadedObjects.Count, "\n",
            "MeshInstance3D   - ", chunkLoader.LoadedChunkPlanes.Count + (chunkLoader.LoadedChunkPlanes.Count * 4), "\n",
            "CollisionShape3D - ", chunkLoader.LoadedObjects.Count + 1, "\n",
            "Frame            - ", Engine.GetFramesPerSecond(), "\n"
        );
    }
}
