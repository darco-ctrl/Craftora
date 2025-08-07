extends TextureRect

@export var inventory: Inventory

func _ready() -> void:
	pass 

func _process(delta: float) -> void:
	invo_inputs()

func invo_inputs()-> void:
	if Input.is_action_just_pressed("toggle_inventory"):
		visible = !visible
