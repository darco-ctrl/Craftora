## Player camera script 
## this is independent to Tick of the game

extends Node3D
class_name Player

@export var camera_ground_pivot: Node3D
@export var camera_pivot: Node3D
@export var camera: Camera3D

@export var speed: float = 3.0 ## Speed which cmaera goes
@export var acceleration: float = 0.1 ## Acceleration of camera
@export var camera_zoom_in_out_speed: float = 20.0 ## Speed at which camera zooms in and out
@export var max_zoom: float = 3.0
@export var max_zoom_out: float = 10

var camera_velocity: Vector3

## >> MAIN FUNCTIONS << ##

func _ready() -> void:
	GameManager.set_player(self)


func _physics_process(delta: float) -> void:
	camera_rotation_position(delta)
	camera_input_movement(delta)

func camera_rotation_position(delta_time: float)-> void:
	camera_pivot.look_at(camera_ground_pivot.position)
	
	var input_direction: int = 0
	if Input.is_action_just_released("zoom_in"):
		input_direction = 1
		
	elif Input.is_action_just_released("zoom_out"):
		input_direction = -1
	
	var forward_direction = -camera_pivot.global_transform.basis.z
	
	var new_position: Vector3 = camera_pivot.position
	
	new_position += forward_direction * camera_zoom_in_out_speed * input_direction * delta_time
	
	#var distance_vector: Vector3 = camera_pivot.global_position - camera_ground_pivot.global_position
	#var distance: float = distance_vector.length()
	
	camera_pivot.position = new_position

func camera_input_movement(delta_time: float) -> void:
	camera_pivot.look_at(camera_ground_pivot.position)
	
	var input_direction := Vector3.ZERO

	if Input.is_action_pressed("move_forward"): input_direction.z -= 1
	if Input.is_action_pressed("move_backward"): input_direction.z += 1
	if Input.is_action_pressed("move_left"): input_direction.x -= 1
	if Input.is_action_pressed("move_right"): input_direction.x += 1

	input_direction = input_direction.normalized()
	
	var y_rotation = camera_pivot.global_transform.basis.get_euler().y
	var flat_rotation = Basis(Vector3.UP, y_rotation)

	var direction = flat_rotation * input_direction

	if input_direction != Vector3.ZERO:
		camera_velocity += direction * acceleration * delta_time
	else:
		camera_velocity = camera_velocity.lerp(Vector3.ZERO, 0.1)

	camera_velocity = camera_velocity.limit_length(speed)

	camera_ground_pivot.position += camera_velocity
