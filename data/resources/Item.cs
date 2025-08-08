using Godot;
using System;

public partial class Item : Resource
{
    [Export] public String ItemName = "";
    [Export] public Texture2D ItemTexture;
}
