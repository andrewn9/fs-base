using Godot;
using System;
using Godot.Collections;

public partial class Grab : Node
{
	private Camera3D camera;
	private float holdDistance = 1.0f;
	private Node3D heldObject = null;
	private Vector3 origin;
	private Vector3 direction;

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("../Camera3D");
	}

	public override void _Process(double delta)
	{
		origin = camera.GlobalTransform.Origin;
		direction = -camera.GlobalTransform.Basis.Z;

		if (Input.IsActionJustPressed("Grab"))
		{
			PhysicsDirectSpaceState3D spaceState = GetViewport().GetWorld3D().DirectSpaceState;
			Vector3 end = origin + direction * holdDistance;

			PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, end);
			query.CollideWithAreas = false;
			query.CollideWithBodies = true;

			Dictionary result = spaceState.IntersectRay(query);

			if (result.Count > 0)
			{
				// GD.Print("Hit result: ", result);

				if (result.TryGetValue("collider", out Variant colliderVariant))
				{
					Node3D collider = colliderVariant.As<Node3D>();

					if (collider is RigidBody3D)
					{
						// GD.Print("Hit a RigidBody3D");
						RigidBody3D body = (RigidBody3D)collider;
						// GD.Print("RigidBody hit: ", body.Name);
						heldObject = body;
					}
				}
			}
		}

		if (!Input.IsActionPressed("Grab") && heldObject != null)
		{
			heldObject = null;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (heldObject != null)
		{
			heldObject.GlobalPosition = camera.GlobalPosition + direction * holdDistance;
		}
    }

}
