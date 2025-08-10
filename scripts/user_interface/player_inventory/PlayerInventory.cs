using Godot;
using System;

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

    public bool Additem(Item item, int count = 1)
    {
        int first_null_index = -1;
        for (int i = 0; i < Inventory.Slots.Count; i++)
        {
            Slot slot = Inventory.Slots[i];
            if (slot.HoldingItem == null && first_null_index == -1)
            {
                first_null_index = i;
            }
            else if (slot.HoldingItem == item)
            {
                slot.ItemCount += count;
                return true;
            }
        }

        if (first_null_index != -1)
        {
            Slot slot = Inventory.Slots[first_null_index];
            slot.HoldingItem = item;
            slot.ItemCount += count;

            return true;
        }

        return false;
    }
}
