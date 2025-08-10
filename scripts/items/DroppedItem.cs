using Godot;
using System;

public partial class DroppedItem : RigidBody3D
{
	GameManager game;

	[Export] public Node3D ItemMeshParent;
	[Export] private Timer DespawnTimer;
	[Export] private Area3D ItemPickUpArea;

	public Item HoldingItem;

	private bool CanPickUp = false;

	private float Frequency = 2; // Speed in wich item move up and down
	private float amplitude = 0.1f; // Maxixmum distant the animation will take the item in up or down direction
	private float time = 0;

	public override void _Ready()
	{
		game = GetTree().Root.GetNode<GameManager>("GameManager");
	}

	public override void _Process(double delta)
	{
		if (!game.ChunkLoader.CanUnloadChunk(game.ChunkLoader.PositionToChunkCoordinates(Position)))
		{
			Sleeping = true;
			Visible = true;
			ItemAnimation((float)delta);
		}
		else
		{
			Sleeping = false;
			Visible = false;
		}
	}

	private void ItemAnimation(float delta)
	{
		var pos = ItemMeshParent.Position;
		pos.Y = get_wave_value(time);
		ItemMeshParent.Position = pos;
		time += delta;

		Vector3 rotation = ItemMeshParent.Rotation;
		rotation.Y += 0.01f;

		ItemMeshParent.Rotation = rotation;
	}

	private float get_wave_value(float t)
	{
		return Mathf.Sin(t * Frequency) * amplitude;
	}

	private void OnTimerTimeout()
	{
		if (CanPickUp)
		{
			QueueFree();
		}
		else
		{
			ItemPickUpArea.Monitorable = true;
			CanPickUp = true;
			DespawnTimer.Start(300);
		}

	}
}
