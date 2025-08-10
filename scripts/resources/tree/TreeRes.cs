using Godot;
using System;

public partial class TreeRes : StaticBody3D
{
	GameManager game;

	[Export] private AnimationPlayer TreeAnimationPlayer;
	[Export] private VisibleOnScreenNotifier3D VisibleOnScreen;
	[Export] private Node3D TreeMesh;

	private Vector3 ItemSpawnPivot;
	private ItemManager.ItemType ItemType = ItemManager.ItemType.Log;

	public Vector2I ChunkPosition = Vector2I.Zero;
	public float TreeHealth = 100.0f;

	public override void _Ready()
	{
		game = GetTree().Root.GetNode<GameManager>("GameManager");


	}


	public override void _Process(double delta)
	{
		if (game.ChunkLoader.CanUnloadChunk(ChunkPosition))
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
			Treebroken();
		}
	}

	private void Treebroken()
	{
		game.ItemManager.SpawnItem(TreeMesh.GlobalPosition + new Vector3(0, 0.3f, 0), ItemType);
		QueueFree();
	}
}
