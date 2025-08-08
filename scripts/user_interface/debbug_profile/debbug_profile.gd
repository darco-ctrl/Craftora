extends Control

@export var frame_displayer: Label

func _process(delta: float) -> void:
	frame_displayer.text = str(Engine.get_frames_per_second())
