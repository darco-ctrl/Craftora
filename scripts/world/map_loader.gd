extends Node3D

## >>> CHUNK VARIABLES <<< ##
@export var render_distance: int = 9 ## Player render distance
@export var chunk_size: Vector2i = Vector2i(16, 16) ## Chunk size in meter
@export var chunk_material: StandardMaterial3D ## plane mesh material used to make chunks

var loaded_chunks: Dictionary[Vector2i, MeshInstance3D] = {}
var render_distance_squared: int
var queued_chunks: Array[Vector2i] = []

func _ready() -> void:
	render_distance_squared = render_distance * render_distance

func _process(delta: float) -> void:
	chunks_loader()
	
func chunks_loader() -> void:
	var new_chunks = make_new_chunk_list()
	for chunk in new_chunks:
		if !queued_chunks.has(chunk):
			queued_chunks.append(chunk)

	render_mesh()

func make_new_chunk_list()-> Array[Vector2i]: # make a list of chunks to be loaded in next tick
	var chunks_to_load: Array[Vector2i]
	var player_chunk_position: Vector2i = position_to_chunk_coordinates(GameManager.player.camera_ground_pivot.position)
	print(player_chunk_position)
	
	for x in range(player_chunk_position.x - render_distance, player_chunk_position.x + render_distance + 1):
		for y in range(player_chunk_position.y - render_distance, player_chunk_position.y + render_distance + 1):
			if is_in_render_distance(x, y):
				var new_chunk: Vector2i = Vector2i(x, y)
				if !loaded_chunks.has(new_chunk):
					chunks_to_load.append(new_chunk)
	
	return chunks_to_load

func render_mesh()-> void:
	if !queued_chunks.is_empty():
		for chunk in queued_chunks.duplicate():
			var new_chunk_mesh: MeshInstance3D = ChunkLoader.create_plane_mesh(chunk, chunk_size, chunk_material)
			add_child(new_chunk_mesh)
			new_chunk_mesh.visible = true
			new_chunk_mesh.position = chunk_coordinates_to_position(chunk)
			loaded_chunks[chunk] = new_chunk_mesh
			queued_chunks.erase(chunk)
			
			var index := queued_chunks.find(chunk)
			if index != -1:
				queued_chunks.remove_at(index)

func is_in_render_distance(x: int, y: int)-> bool:
	return x * x + y * y <= render_distance_squared

func position_to_chunk_coordinates(pos: Vector3)-> Vector2i:
	return Vector2i(floor(pos.x / chunk_size.y), floor(pos.y / chunk_size.y))
	
func chunk_coordinates_to_position(chunk_pos: Vector2i)-> Vector3:
	return Vector3(floor(chunk_pos.x * chunk_size.y), 0, floor(chunk_pos.y * chunk_size.y))
