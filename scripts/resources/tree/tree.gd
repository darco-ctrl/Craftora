extends StaticBody3D
class_name ResourceTree

@export var tree_animation_player: AnimationPlayer
var chunk_position: Vector2i

var health = 100

func attacked()-> void:
	health -= 10
	tree_animation_player.play("tree_attacked")
	
	if health <= 0:
		ChunkLoader.loaded_objects.erase(ChunkLoader.position_to_chunk_coordinates(position))
		
		queue_free()
