extends Node

var a = false
func _unhandled_input(event: InputEvent) -> void:
	if Input.is_action_just_pressed("quit"):
		a = !a;
		if a:
			Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		else:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
