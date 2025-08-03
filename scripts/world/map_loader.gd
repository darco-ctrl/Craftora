extends Node3D

## >>> CHUNK VARIABLES <<< ##
@export var render_distance: int = 9

var queued_chunks: Array[Vector2]
var loaded_chunks: Array[Vector2]

func _ready() -> void:
	pass # Replace with function body.



func _process(delta: float) -> void:
	pass

func make_new_chunk_list()-> void: # make a list of chunks to be loaded in next tick
	pass
