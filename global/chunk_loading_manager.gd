extends Node

var map_loader: MapLoader

var render_distance: int = 3 ## Player render distance
var render_distance_squared: int
var chunk_size: Vector2i = Vector2i(16, 16) ## Chunk size in meter

## Resources Noise
var noise: FastNoiseLite
var frequency: float = 0.04
var seed: int
var noise_type: int = FastNoiseLite.TYPE_PERLIN
var tree_spawn_strenght: float = 3

var loaded_chunks: Dictionary[Vector2i, MeshInstance3D] = {}
var loaded_objects: Dictionary[Vector2i, ResourceTree] = {}

const MAX_UNLOADS_PER_FRAME := 10

var queued_chunks: Array[Vector2i] = []
var queued_objects: Dictionary[Vector2i, ResourcesQueueData] = {} ## var queued_objects: Dictionary[Object_position: Vector2i, Objects_Data: ResourcesQueueData] = {}

var chunks_to_unload: Array[Vector2i] = []
var objects_to_unload: Array[Vector2i] = []

enum ItemType {
	TREE,
	STONE
}

enum ChunkLoadPhase {
	QUEUE_CHUNKS_PHASE,
	QUEUE_RES_PHASE,
	RENDER_QUEUED_OBJECTS_PHASE,
	#BLANK
}

var neon_: int = 0

class ResourcesQueueData: # pos and item type of this resources used in "queued_objects" variables
	var chunk_pos: Vector2i
	var item_type: int
	
	func _init(p: Vector2i, it: int) -> void:
		chunk_pos = p
		item_type = it

func _ready() -> void:
	render_distance_squared = render_distance * render_distance
	
	seed = abs(randi())

func _process(delta: float) -> void:
	chunk_unloader_process()

func setup_reosources_nosie()-> void:
	noise = FastNoiseLite.new()
	
	noise.noise_type = noise_type
	noise.frequency = frequency
	noise.seed = seed
			
#region PHASE ONE

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

func chunk_unloader_prepare() -> void:
	for chunk in loaded_chunks:
		if can_unload_chunk(chunk):
			var plane = loaded_chunks[chunk]
			if is_instance_valid(plane):
				chunks_to_unload.append(chunk)
			else:
				loaded_objects.erase(chunk)
	
	for object_pos in loaded_objects:
		var object = loaded_objects[object_pos]
		if is_instance_valid(object):
			if !loaded_chunks.has(loaded_objects[object_pos].chunk_position):
				objects_to_unload.append(object_pos)
		else:
			loaded_objects.erase(object_pos)

func chunk_unloader_process() -> void:
	for i in range(MAX_UNLOADS_PER_FRAME):
		if chunks_to_unload.size() > 0:
			var chunk = chunks_to_unload.pop_back()
			if loaded_chunks.has(chunk):
				loaded_chunks[chunk].queue_free()
				loaded_chunks.erase(chunk)

		if objects_to_unload.size() > 0:
			var obj = objects_to_unload.pop_back()
			if loaded_objects.has(obj):
				loaded_objects[obj].queue_free()
				loaded_objects.erase(obj)

#endregion

#region PHASE TWO

# Create Tree
func create_resource_queue()-> void:
	for chunk in queued_chunks:
		var chunk_from: Vector2 = chunk_coordinates_to_position(chunk, true)
		var chunk_to: Vector2 = chunk_from + Vector2(chunk_size.x, chunk_size.y)
		
		for x in range(chunk_from.x, chunk_to.x):
			for y in range(chunk_from.y, chunk_to.y):
				var new_position: Vector2i = Vector2i(x, y)
				if !loaded_objects.has(new_position):
					var noise_value: float = noise.get_noise_2d(x, y)
					if noise_value > 0.1:
						var spawn_chance := pow((noise_value + 1.0) / 2.0, tree_spawn_strenght)

						if randf() < spawn_chance:
							var new_res_data = ResourcesQueueData.new(chunk, ItemType.TREE)
							queued_objects[new_position] = new_res_data
					
				#elif noise_value < 0.25:
					#var new_res_data = ResourcesQueueData.new(Vector2i(x, y), ItemType.STONE)
					#queued_objects[chunk] = new_res_data

#endregion


#region PHASE THREE 

func render_ground_mesh()-> void:
	if !queued_chunks.is_empty():
		for chunk in queued_chunks.duplicate():
			var new_chunk_mesh: MeshInstance3D = create_plane_mesh(chunk_size)
			map_loader.grounds.add_child(new_chunk_mesh)
			new_chunk_mesh.visible = true
			new_chunk_mesh.position = chunk_coordinates_to_position(chunk)
			loaded_chunks[chunk] = new_chunk_mesh
			
			var index := queued_chunks.find(chunk)
			if index != -1:
				queued_chunks.remove_at(index)

func create_plane_mesh(plane_size: Vector2)-> MeshInstance3D:
	var chunk_mesh: MeshInstance3D = MeshInstance3D.new()
	
	var plane_mesh = PlaneMesh.new()
	plane_mesh.size = plane_size
	plane_mesh.material = map_loader.ground_material
	
	plane_mesh.center_offset = Vector3(plane_size.x / 2, 0, plane_size.y / 2)
	
	chunk_mesh.mesh = plane_mesh
	
	return chunk_mesh

func tree_loader()-> void:
	for block_pos in queued_objects.duplicate():
		var current_object: ResourcesQueueData = queued_objects[block_pos]
		var item: ResourceTree = map_loader.Object_Array[current_object.item_type].instantiate()
		item.chunk_position = current_object.chunk_pos
		
		loaded_objects[block_pos] = item
		item.position = Vector3(block_pos.x, 0, block_pos.y)
		item.visible = true
		map_loader.resources.add_child(item)
		
		queued_objects.erase(block_pos)

#endregion

#region >> Supporting Fcuntions <<
func is_in_radius(x: int, y: int)-> bool:
	return x * x + y * y <= render_distance_squared

func position_to_chunk_coordinates(pos: Vector3)-> Vector2i:
	return Vector2i(floor(pos.x / chunk_size.y), floor(pos.z / chunk_size.y))
	
func chunk_coordinates_to_position(chunk_pos: Vector2i, in_vector2=false)-> Variant:
	if in_vector2:
		return Vector2i(floor(chunk_pos.x * chunk_size.y), floor(chunk_pos.y * chunk_size.y))
	else:
		return Vector3i(floor(chunk_pos.x * chunk_size.y), 0, floor(chunk_pos.y * chunk_size.y))

func can_unload_chunk(chunk_pos: Vector2i)-> bool:
	if position_to_chunk_coordinates(GameManager.player.camera_ground_pivot.position).distance_to(chunk_pos) > render_distance:
		return true
	return false

#endregion
