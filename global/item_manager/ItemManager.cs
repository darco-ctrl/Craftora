using Godot;
using Godot.Collections;
using System;

public partial class ItemManager : Node
{
	public Node3D DroppedItemsParent;

	public enum ItemType
	{
		None,
		Log
	}

	public enum ItemUse
	{
		None,
		Placable,
		Consumable,
	}

	[Export] public PackedScene ItemDroppedScene;

	[Export] public Array<Item> Items = [];

	float MinForceAngle = Mathf.DegToRad(90 - 35);
	float MaxForceAngle = Mathf.DegToRad(90 + 35);

	public void SpawnItem(Vector3 pos, ItemType item_type)
	{
		DroppedItem newItem = (DroppedItem)ItemDroppedScene.Instantiate();
		//GD.Print((int)item_type);
		Item item = Items[(int)item_type];
		Node3D mesh = (Node3D)item.ItemMesh.Instantiate();

		newItem.HoldingItem = item;

		DroppedItemsParent.AddChild(newItem);
		newItem.ItemMeshParent.AddChild(mesh);

		GD.Print(pos);
		newItem.Position = pos;
		newItem.Visible = true;

	}
}
