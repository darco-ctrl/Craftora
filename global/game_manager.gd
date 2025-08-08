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
	#run_ray_check()
	pass
	
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

			
