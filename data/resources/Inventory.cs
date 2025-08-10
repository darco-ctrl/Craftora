using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Inventory : Resource
{
    public Godot.Collections.Array<Slot> Slots = [];
}
