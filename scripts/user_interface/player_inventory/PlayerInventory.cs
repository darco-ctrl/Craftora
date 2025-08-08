using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerInventory : Control
{
    [Export] public Inventory Inventory;
    [Export] public TextureRect InventoryUi;
    [Export] public TextureRect HotbarUi;

    private int HotbarSize = 8;
    private bool IsInventoryOpen = false;

    public override void _Ready()
    {
        Inventory.Slots.Resize(58);
        Inventory.Slots = SetupSlots();
        InventoryUi.Visible = false;
    }

    private Godot.Collections.Array<Slot> SetupSlots()
    {
        GridContainer hotbarGrid = (GridContainer)HotbarUi.GetChild(0);
        GridContainer invoGrid = (GridContainer)InventoryUi.GetChild(0);

        var hotbarSlots = hotbarGrid.GetChildren();
        var invoSlots = invoGrid.GetChildren();

        foreach (var slot in hotbarSlots)
            ((Slot)slot).IsOpen = true;

        var returnInvoSlots = new Godot.Collections.Array<Slot>();

        foreach (var hSlot in hotbarSlots)
            returnInvoSlots.Add((Slot)hSlot);

        foreach (var iSlot in invoSlots)
            returnInvoSlots.Add((Slot)iSlot);

        GD.Print(returnInvoSlots.Count);
        return returnInvoSlots;
    }

    public override void _Process(double delta)
    {
        InvoInputs();
    }

    private void InvoInputs()
    {
        if (Input.IsActionJustPressed("toggle_inventory"))
            ToggleInvo();
    }

    private void ToggleInvo()
    {
        if (IsInventoryOpen)
        {
            for (int i = 8; i < Inventory.Slots.Count; i++)
            {
                Slot slot = (Slot)Inventory.Slots[i];
                slot.IsOpen = true;
            }

            IsInventoryOpen = !IsInventoryOpen;
            InventoryUi.Visible = IsInventoryOpen;
        }
        else
        {
            for (int i = 8; i < Inventory.Slots.Count; i++)
            {
                Slot slot = (Slot)Inventory.Slots[i];
                slot.IsOpen = false;
            }

            IsInventoryOpen = !IsInventoryOpen;
            InventoryUi.Visible = IsInventoryOpen;
        }
    }
}
