extends Node3D

@export var item_collision: Area3D
@export var selection_box: Node3D
@export var camera: Camera3D

var ray_query: PhysicsRayQueryParameters3D
var space_state: PhysicsDirectSpaceState3D

## >> MAIN FUNCTIONS << ##
func _ready()-> void:
	initialize_ray()

func _process(_delta: float) -> void:
	update_ray_cast()
	run_ray_check()

## >> --------------- << ##

func initialize_ray()-> void:
	space_state = get_world_3d().direct_space_state
	ray_query = PhysicsRayQueryParameters3D.new()
	
	ray_query.collision_mask = 1 # interact collision : most probabl evey object has this on

func update_ray_cast()-> void:
	var mouse_pos: Vector2 = get_viewport().get_mouse_position()
	var from: Vector3 = camera.project_ray_origin(mouse_pos)
	var to: Vector3 = from + camera.project_ray_normal(mouse_pos) * GameManager.player_range
	
	ray_query.from = from
	ray_query.to = to
	
	var raycast_result = space_state.intersect_ray(ray_query)
	
	if !raycast_result.is_empty():
		var collider = raycast_result["collider"]
		if collider.is_in_group("block"):
			selection_box.position = collider.position
		else:
			var pos = raycast_result["position"]
			
			var y: int
			if pos.y < 0: y = 0
			else: y = int(pos.y)
			
			item_collision.position = pos
			selection_box.position = Vector3(grid(pos.x), y - 1, grid(pos.z))
			selection_box.visible = true
	else:
		selection_box.visible = false

func run_ray_check()-> void:
	if ray_query:
		
		var raycast_result = space_state.intersect_ray(ray_query)
		if raycast_result:
			var collider: Node3D = raycast_result["collider"]
			
			if collider.is_in_group("interactable"):
				if Input.is_action_just_pressed("attack"):
					collider.attacked()
	

## >> SUPPORT FUNCTIONS << ##

func grid(num: float)-> int:
	var int_num: int
	if num < 0:
		int_num = int(num - 1)
	else:
		int_num = int(num)
	
	return int_num
