using Godot;
using System;

public partial class TreeRes : StaticBody3D
{
	ChunkLoader chunkLoader;

	[Export] private AnimationPlayer TreeAnimationPlayer;
	[Export] private VisibleOnScreenNotifier3D VisibleOnScreen;
	[Export] private Node3D TreeMesh;

	public Vector2I ChunkPosition;
	public float TreeHealth = 100.0f;

    public override void _Ready()
    {
		chunkLoader = GetTree().Root.GetNode<ChunkLoader>("ChunkLoader");
    }


    public override void _Process(double delta)
	{
		if (chunkLoader.CanUnloadChunk(ChunkPosition))
		{
			QueueFree();
		}
	}


	public void Clicked()
	{

		if (VisibleOnScreen.IsOnScreen())
		{
			TreeMesh.Visible = true;
		}
		else
		{
			TreeMesh.Visible = false;
		}


		TreeHealth -= 10.0f;
		TreeAnimationPlayer.Play("tree_attacked");

		if (TreeHealth <= 0.0f)
		{
			QueueFree();
		}
	}
}
