extends Node


func create_plane_mesh(chunk_pos: Vector2, plane_size: Vector2, mat: StandardMaterial3D)-> MeshInstance3D:
	var chunk_mesh: MeshInstance3D = MeshInstance3D.new()
	
	var plane_mesh = PlaneMesh.new()
	plane_mesh.size = plane_size
	plane_mesh.material = mat
	plane_mesh.center_offset = Vector3(plane_size.x / 2, 0, plane_size.y / 2)
	
	chunk_mesh.mesh = plane_mesh
	
	return chunk_mesh
