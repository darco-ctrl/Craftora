extends StaticBody3D
class_name ResourceTree

var chunk_position: Vector2i

var health = 100

func attacked()-> void:
	health -= 10
	if health <= 0:
		ChunkLoader.loaded_objects.erase(ChunkLoader.position_to_chunk_coordinates(position))
		
		queue_free()
