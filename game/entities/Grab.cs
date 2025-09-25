using Godot;
using System;
using Godot.Collections;

public partial class Grab : Node
{
	private Camera3D camera;
	private float holdDistance = 1.0f;
	private float grabStrength = 45.0f;
	private RigidBody3D heldObject = null;
	private Vector3 origin;
	private Vector3 direction;

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("../Camera3D");
	}

	public void queryGrab()
	{
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
						if (body.IsInGroup("grabbed"))
						{
							return;
						}
						// GD.Print("RigidBody hit: ", body.Name);
						body.GravityScale = 0;
						Rpc(nameof(_requestGrab), body.GetPath());
						heldObject = body;
					}
				}
			}
		}
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void _requestGrab(NodePath bodyPath)
	{
		Node3D node = GetNode<Node3D>(bodyPath);
		if (node is RigidBody3D body)
		{
			body.SetMultiplayerAuthority(Multiplayer.GetRemoteSenderId());
			body.AddToGroup("grabbed");
		}
	}
	
	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
	private void _releaseGrab(NodePath bodyPath)
	{
		Node3D node = GetNode<Node3D>(bodyPath);
		if (node is RigidBody3D body)
		{
			body.SetMultiplayerAuthority(1);
			body.RemoveFromGroup("grabbed");
		}
	}

	public override void _Process(double delta)
	{
		origin = camera.GlobalTransform.Origin;
		direction = -camera.GlobalTransform.Basis.Z;

		queryGrab();

		if ((!Input.IsActionPressed("Grab")) && heldObject != null)
		{
			Rpc(nameof(_releaseGrab), heldObject.GetPath());
			heldObject.GravityScale = 1;
			heldObject = null;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (heldObject != null)
		{
			Vector3 toPosition = camera.GlobalPosition + direction * holdDistance;
			heldObject.LinearVelocity = grabStrength * (toPosition - heldObject.GlobalPosition);
		}
    }

}
