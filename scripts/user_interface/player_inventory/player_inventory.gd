extends Control

@export var inventory: Inventory

@export var inventory_ui: TextureRect
@export var hotbar_ui: TextureRect

var hotbar_size

func _ready() -> void:
	inventory.slots = setup_slots()
	#inventory_ui	.visible = false

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
	
	return return_invo_slots

func _process(delta: float) -> void:
	invo_inputs()

func invo_inputs()-> void:
	if Input.is_action_just_pressed("toggle_inventory"):
		inventory_ui.visible = !inventory_ui.visible
