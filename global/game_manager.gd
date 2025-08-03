extends Node

const TICK_PER_SECOND: int = 20
const TICK_INTERVAL: float = 0.05
var tick: int = 0
var tick_time: float = 0

var player: Player

func _physics_process(delta: float) -> void:
	update_tick(delta)
	

func update_tick(delta_time: float) -> void:
	tick_time += delta_time
	if tick_time >= TICK_INTERVAL:
		tick_time = 0
		tick += 1
		
		if tick >= TICK_PER_SECOND:
			tick = 0

func set_player(plr: Player)-> void:
	player = plr
