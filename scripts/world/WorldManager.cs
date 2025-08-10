using Godot;
using System;

public partial class WorldManager : Node3D
{
    GameManager Game;

    [Export] public Node3D Grounds;
    [Export] public Node3D Resources;
    [Export] public Node3D PlacedObjects;
    [Export] public Node3D ItemsParent;

    [Export] public Godot.Collections.Array<PackedScene> Object_Array;

    [Export] public StandardMaterial3D Ground_Material;

    public override void _Ready()
    {
        Game = (GameManager)GetTree().Root.GetNode("GameManager");

        Game.ItemManager.DroppedItemsParent = ItemsParent;
        Game.ChunkLoader.WorldRoot = this;
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
            "StaticBody3D     - ", Game.ChunkLoader.LoadedObjects.Count, "\n",
            "MeshInstance3D   - ", Game.ChunkLoader.LoadedChunkPlanes.Count + (Game.ChunkLoader.LoadedChunkPlanes.Count * 4), "\n",
            "CollisionShape3D - ", Game.ChunkLoader.LoadedObjects.Count + 1, "\n",
            "Frame            - ", Engine.GetFramesPerSecond(), "\n"
        );
    }
}
