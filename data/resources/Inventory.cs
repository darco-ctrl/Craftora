using Godot;
using Godot.Collections;
using System;

[GlobalClass]
public partial class Inventory : Resource
{
    public Array<Slot> Slots = new Array<Slot>();
}
