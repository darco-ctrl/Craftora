extends Node3D

@export var render_distance: int = 9 ## Player render distance
@export var chunk_size: Vector2i = Vector2i(16, 16) ## Chunk size in meter
@export var chunk_material: StandardMaterial3D ## plane mesh material used to make chunks

var loaded_chunks: Dictionary[Vector2i, MeshInstance3D] = {}
var render_distance_squared: int
var queued_chunks: Array[Vector2i] = []

var switch_: bool = false

## >> MAIN FUNCTIONS << ##

func _ready() -> void:
	render_distance_squared = render_distance * render_distance

func _process(delta: float) -> void:
	if GameManager.is_game_tick_even():
		queued_chunks = make_new_chunk_list()
		chunk_unloader()
	else:
		render_mesh()

func chunk_unloader() -> void:
	for chunk in loaded_chunks:
		if can_unload_chunk(chunk):
			loaded_chunks[chunk].queue_free()
			loaded_chunks.erase(chunk)

func make_new_chunk_list()-> Array[Vector2i]: # make a list of chunks to be loaded in next tick
	var chunks_to_load: Array[Vector2i]
	var player_chunk_position: Vector2i = position_to_chunk_coordinates(GameManager.player.camera_ground_pivot.position)
	
	for x in range(-render_distance, render_distance + 1):
		for y in range(-render_distance, render_distance + 1):
			if is_in_radius(x, y):
				var new_chunk: Vector2i = Vector2i(x + player_chunk_position.x, y + player_chunk_position.y)
				if !loaded_chunks.has(new_chunk):
					chunks_to_load.append(new_chunk)
	
	return chunks_to_load

func render_mesh()-> void:
	if !queued_chunks.is_empty():
		for chunk in queued_chunks.duplicate():
			var new_chunk_mesh: MeshInstance3D = create_plane_mesh(chunk, chunk_size)
			add_child(new_chunk_mesh)
			new_chunk_mesh.visible = true
			new_chunk_mesh.position = chunk_coordinates_to_position(chunk)
			loaded_chunks[chunk] = new_chunk_mesh
			
			var index := queued_chunks.find(chunk)
			if index != -1:
				queued_chunks.remove_at(index)


func create_plane_mesh(chunk_pos: Vector2, plane_size: Vector2)-> MeshInstance3D:
	var chunk_mesh: MeshInstance3D = MeshInstance3D.new()
	
	var plane_mesh = PlaneMesh.new()
	plane_mesh.size = plane_size
	plane_mesh.material = chunk_material
	
	plane_mesh.center_offset = Vector3(plane_size.x / 2, 0, plane_size.y / 2)
	
	chunk_mesh.mesh = plane_mesh
	
	return chunk_mesh

## >> SUPPORTING FUNCTIONS << ##

func is_in_radius(x: int, y: int)-> bool:
	return x * x + y * y <= render_distance_squared

func position_to_chunk_coordinates(pos: Vector3)-> Vector2i:
	return Vector2i(floor(pos.x / chunk_size.y), floor(pos.z / chunk_size.y))
	
func chunk_coordinates_to_position(chunk_pos: Vector2i)-> Vector3:
	return Vector3(floor(chunk_pos.x * chunk_size.y), 0, floor(chunk_pos.y * chunk_size.y))

func can_unload_chunk(chunk_pos: Vector2i)-> bool:
	if position_to_chunk_coordinates(GameManager.player.camera_ground_pivot.position).distance_to(chunk_pos) > render_distance:
		return true
	return false
