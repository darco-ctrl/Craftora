using Godot;
using System;

public partial class Slot : TextureRect
{
	[Export] TextureRect ItemTexture;
	[Export] Label ItemCounterDisplay;

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

			if (ItemCount != 0)
			{
				ItemCounterDisplay.Text = ItemCount.ToString();
			}
			else
			{
				ItemCounterDisplay.Text = "";
			}
		}
    }

}
