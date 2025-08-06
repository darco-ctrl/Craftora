extends Node3D

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	GameManager.space_state = get_world_3d().direct_space_state

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
