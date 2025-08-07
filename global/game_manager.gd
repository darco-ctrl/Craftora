extends Node

const TICK_PER_SECOND: int = 30
const TICK_INTERVAL: float = 0.0333
var tick: int = 0
var tick_time: float = 0

var player: Player

var ray_query: PhysicsRayQueryParameters3D
var space_state: PhysicsDirectSpaceState3D

var player_range = 100

func _ready()-> void:
	initialize_ray()

func _physics_process(delta: float) -> void:
	update_tick(delta)

func _process(delta: float) -> void:
	run_ray_check()

func initialize_ray()-> void:
	ray_query = PhysicsRayQueryParameters3D.new()
	
	ray_query.collision_mask = 1

func update_tick(delta_time: float) -> void:
	tick_time += delta_time
	if tick_time >= TICK_INTERVAL:
		tick_time = 0
		tick += 1
		
		if tick >= TICK_PER_SECOND:
			tick = 0

func set_player(plr: Player)-> void:
	player = plr

func is_game_tick_even()-> bool:
	if tick % 2 == 0:
		return true
	else:
		return false
	
func run_ray_check()-> void:
	if ray_query:
		var mouse_pos: Vector2 = get_viewport().get_mouse_position()
		var from: Vector3 = player.camera.project_ray_origin(mouse_pos)
		var to: Vector3 = from + player.camera.project_ray_normal(mouse_pos) * player_range
		
		ray_query.from = from
		ray_query.to = to
		
		var raycast_result = space_state.intersect_ray(ray_query)
		var collider: Node3D = raycast_result["collider"]
		
		if collider.is_in_group("interactable"):
			if Input.is_action_just_pressed("attack"):
				collider.attacked()
		
