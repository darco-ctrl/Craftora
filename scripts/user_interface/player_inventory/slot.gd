extends TextureRect
class_name Slot

@export var item_texture: TextureRect

var item_holding: Item
var item_count: int
var is_open: bool = false

func _process(delta: float) -> void:
	if is_open and item_holding:
		if item_holding.item_texture != item_texture.texture:
			item_texture.texture = item_holding.item_texture
	
