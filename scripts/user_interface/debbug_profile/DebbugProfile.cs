using Godot;
using System;

public partial class DebbugProfile : Control
{
	[Export] Label FrameRateLabel;

    public override void _Process(double delta)
    {
        FrameRateLabel.Text = $"FPS: {Engine.GetFramesPerSecond():0.00}";
    }
}
