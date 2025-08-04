extends Node3D

@export var selection_box: Node3D
@export var camera: Camera3D

@export var player_interaction_range: int = 100

var space_state: PhysicsDirectSpaceState3D
var ray_query: PhysicsRayQueryParameters3D

## >> MAIN FUNCTIONS << ##

func _ready() -> void:
	initialize_ray()

func _process(delta: float) -> void:
	update_ray_cast()

## >> --------------- << ##

func initialize_ray()-> void:
	space_state = get_world_3d().direct_space_state
	ray_query = PhysicsRayQueryParameters3D.new()
	
	ray_query.collision_mask = 1 # interact collision : most probabl evey object has this on

func update_ray_cast()-> void:
	var mouse_pos: Vector2 = get_viewport().get_mouse_position()
	var from: Vector3 = camera.project_ray_origin(mouse_pos)
	var to: Vector3 = from + camera.project_ray_normal(mouse_pos) * player_interaction_range
	
	ray_query.from = from
	ray_query.to = to
	
	var raycast_result = space_state.intersect_ray(ray_query)
	
	if !raycast_result.is_empty():
		var pos = raycast_result["position"]
		var y: int
		
		if pos.y < 0: y = 0
		else: y = int(pos.y)
		
		selection_box.position = Vector3(floor(pos.x), y - 1, floor(pos.z))
		selection_box.visible = true
	else:
		selection_box.visible = false
	
	

## >> SUPPORT FUNCTIONS << ##
