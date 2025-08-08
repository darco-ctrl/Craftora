extends Control

@export var inventory: Inventory

@export var inventory_ui: TextureRect
@export var hotbar_ui: TextureRect

var hotbar_size: int = 8
var is_inventory_open: bool = false

func _ready() -> void:
	inventory.slots.resize(58)
	inventory.slots = setup_slots()
	inventory_ui	.visible = false

func setup_slots()-> Array:
	var hotbar_grid: GridContainer = hotbar_ui.get_child(0)
	var invo_grid: GridContainer = inventory_ui.get_child(0)
	
	var hotbar_slots: Array = hotbar_grid.get_children()
	var invo_slots: Array = invo_grid.get_children()
	
	for slots: Slot in hotbar_slots:
		slots.is_open = true
	
	var return_invo_slots: Array[Slot]
	
	for h_slot: Slot in hotbar_slots:
		return_invo_slots.append(h_slot)
	
	for i_slot: Slot in invo_slots:
		return_invo_slots.append(i_slot)
	
	print(return_invo_slots.size())
	return return_invo_slots

func _process(delta: float) -> void:
	invo_inputs()

func invo_inputs()-> void:
	if Input.is_action_just_pressed("toggle_inventory"):
		toggle_invo()

func toggle_invo()-> void:
	if is_inventory_open:
		
		for i in range(8, inventory.slots.size()):
			var slot: Slot = inventory.slots[i]
			slot.is_open = true
			
		is_inventory_open = !is_inventory_open
		inventory_ui.visible = is_inventory_open
	else:
		
		for i in range(8, inventory.slots.size()):
			var slot: Slot = inventory.slots[i]
			slot.is_open = false
		
		is_inventory_open = !is_inventory_open
		inventory_ui.visible = is_inventory_open
