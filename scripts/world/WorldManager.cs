using Godot;
using System;

public partial class WorldManager : Node3D
{
    GameManager GM;
    ChunkLoader CL;

    [Export] public Node3D Grounds;
    [Export] public Node3D Resources;
    [Export] public Node3D PlacedObjects;
    [Export] public Godot.Collections.Array<PackedScene> Object_Array;

    [Export] public StandardMaterial3D Ground_Material;

    public override void _Ready()
    {
        GM = (GameManager)GetTree().Root.GetNode("GameManager");
        CL = (ChunkLoader)GetTree().Root.GetNode("ChunkLoader");

        CL.WorldManager = this;
        CL.SetupReosourcesNoise();
    }

    public override void _Process(double _delta)
    {

        if (Input.IsActionJustPressed("exclamation"))
        {
            Debbug_Print();
        }

        switch ((int)(GM.Tick % 3))
        {
            case (int)ChunkLoader.ChunkLoadPhase.PHASE_ONE:
                Phase_One();
                break;
            case (int)ChunkLoader.ChunkLoadPhase.PHASE_TWO:
                Phase_Three();
                break;
            // case (int)ChunkLoader.ChunkLoadPhase.BLANK:
            //     Phase_Four();
            //     break;
        }
    }

    public void Phase_One()
    {
        CL.QueuedChunks = CL.MakeNewChunkList();
        CL.ChunkUnloaderPrepare();
    }

    public void Phase_Three()
    {
        CL.RenderGroundMesh();
    }

    public void Debbug_Print()
    {
        GD.Print(
            "------------- CURRENTLY LOADED OBJECTS -------------", "\n",
            "StaticBody3D     - ", CL.LoadedObjects.Count, "\n",
            "MeshInstance3D   - ", CL.LoadedChunks.Count + (CL.LoadedChunks.Count * 4), "\n",
            "CollisionShape3D - ", CL.LoadedObjects.Count + 1, "\n",
            "Frame            - ", Engine.GetFramesPerSecond(), "\n",
            "Render Distance  - ", CL.RenderDistance, "\n", " ------------------- "
        );
    }
}
