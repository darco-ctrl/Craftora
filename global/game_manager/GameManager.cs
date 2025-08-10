using Godot;
using System;

public partial class GameManager : Node
{

	public ChunkLoader ChunkLoader;
	public ItemManager ItemManager;

	const int TICK_PER_SECOND = 20;
	const float TICK_INTERVAL = 0.5f;
	public int Tick = 0;
	private float TickTime = 0.0f;

	public PlayerCamera Player;

	public int PlayerRange = 100;

    public override void _Ready()
    {
		InitializeAutoloads();
    }


	public override void _Process(double delta)
	{
		UpdateTick((float)delta);
	}

	private void InitializeAutoloads()
	{
		ChunkLoader = GetTree().Root.GetNode<ChunkLoader>("ChunkLoader");
		ItemManager = GetTree().Root.GetNode<ItemManager>("ItemManager");
	}

	private void UpdateTick(float delta)
	{
		TickTime += (float)delta;
		if (TickTime >= TICK_INTERVAL)
		{
			Tick++;
			TickTime -= TICK_INTERVAL;

			if (Tick >= TICK_PER_SECOND)
			{
				Tick = 0;
			}
		}
	}

	public void SetPlayer(PlayerCamera player)
	{
		Player = player;
	}
}
