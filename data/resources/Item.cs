using Godot;
using System;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public String ItemName = "";
    [Export] public ItemManager.ItemType ItemType = ItemManager.ItemType.None;
    [Export] public ItemManager.ItemUse ItemUse = ItemManager.ItemUse.None;
    [Export] public Texture2D ItemTexture;
    [Export] public PackedScene ItemMesh;
}
