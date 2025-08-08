using Godot;
using System;

public partial class Slot : TextureRect
{
	[Export] TextureRect ItemTexture;

	public Item HoldingItem;
	public int ItemCount = 0;
	public bool IsOpen = false;

	public override void _Process(double delta)
	{
		if (IsOpen && HoldingItem != null)
		{
			if (HoldingItem.ItemTexture != ItemTexture.Texture)
			{
				ItemTexture.Texture = HoldingItem.ItemTexture;
			}
		}
    }

}
