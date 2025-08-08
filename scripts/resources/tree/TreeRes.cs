using Godot;
using System;

public partial class TreeRes : StaticBody3D
{
	[Export] private AnimationPlayer TreeAnimationPlayer;
	[Export] private VisibleOnScreenNotifier3D VisibleOnScreen;
	[Export] private Node3D TreeMesh;

	public Vector2I ChunkPosition;
	public float TreeHealth = 100.0f;

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
