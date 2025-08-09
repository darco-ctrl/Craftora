using Godot;
using System;

public partial class GameManager : Node
{

	const int TICK_PER_SECOND = 40;
	const float TICK_INTERVAL = 0.025f;
	public int Tick = 0;
	private float TickTime = 0.0f;

	public PlayerCamera Player;

	public int PlayerRange = 100;

	public override void _Process(double delta)
	{
		UpdateTick((float)delta);
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
