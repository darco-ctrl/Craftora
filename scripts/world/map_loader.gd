extends Node3D
class_name MapLoader

@export var grounds: Node3D
@export var resources: Node3D
@export var placed_objects: Node3D
@export var Object_Array: Array[PackedScene]

@export var ground_material: StandardMaterial3D ## plane mesh material used to make chunks

func _ready() -> void:
	ChunkLoader.map_loader = self
	ChunkLoader.setup_reosources_nosie()
	print(ground_material)

func _process(_delta: float) -> void:
	ChunkLoader.neon_ += 1
	if Input.is_action_just_pressed("exclamation"):
		debbug_print()
	
	match GameManager.tick % ChunkLoader.ChunkLoadPhase.size():
		ChunkLoader.ChunkLoadPhase.QUEUE_CHUNKS_PHASE: phase_one()
		ChunkLoader.ChunkLoadPhase.QUEUE_RES_PHASE: phase_two()
		ChunkLoader.ChunkLoadPhase.RENDER_QUEUED_OBJECTS_PHASE: phase_three()
		#ChunkLoader.ChunkLoadPhaseBLANK: phase_four()

func  phase_one()-> void:
	ChunkLoader.queued_chunks = ChunkLoader.make_new_chunk_list()
	ChunkLoader.chunk_unloader_prepare()

func phase_two()-> void:	
	ChunkLoader.create_resource_queue()

func phase_three()-> void:
	ChunkLoader.tree_loader()
	ChunkLoader.render_ground_mesh()

#func phase_four()-> void:
	#pass

func debbug_print()-> void:
	print(
		"------------- CURRENTLY LOADED OBJECTS -------------", "\n",
		"StaticBody3D     - ", ChunkLoader.loaded_objects.size(), "\n",
		"MeshInstance3D   - ", ChunkLoader.loaded_chunks.size() + (ChunkLoader.loaded_objects.size() * 4), "\n",
		"CollisionShape3D - ", ChunkLoader.loaded_objects.size() + 1, "\n",
		"Frame            - ", Engine.get_frames_per_second(), "\n",
		"Render Distance  - ", ChunkLoader.render_distance, "\n", " ------------------- "
		)
