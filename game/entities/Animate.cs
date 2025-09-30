using Godot;
using System;
public partial class Animate : Node3D
{
	// Called when the node enters the scene tree for the first time.
	private Movement movement;
	private AnimationPlayer animationPlayer;
    private Skeleton3D skeleton;
	private IkController ikController;
	private Grab grab;
	public override void _Ready()
	{
		movement = GetParent<Movement>();
		skeleton = GetNode<Skeleton3D>("Skeleton3D");
		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		grab = GetParent().GetNode<Grab>("Grab");
		ikController = GetNode<IkController>("IKController");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (movement.Velocity.Length() > 0.3f)
		{
			if (!animationPlayer.IsPlaying() || animationPlayer.CurrentAnimation != "Run")
			{
				animationPlayer.Play("Run");
			}
		}
		else
		{
			if (!animationPlayer.IsPlaying() || animationPlayer.CurrentAnimation != "Idle")
			{
				animationPlayer.Play("Idle");
			}
		}

		// Make head bone follow look direction
		ikController.headLookVector = movement.LookVector % (2 * Mathf.Pi);
		
		// // Make arms grab toPosition
		// if (grab.heldObject != null)
		// {
		// 	Transform3D rightArmTransform = skeleton.GetBoneGlobalPose(rightLowerArmBoneIndex);
		// 	GD.Print($"initial: {rightArmTransform}");

		// 	GD.Print($"grab toPosition: {grab.toPosition}");
		// 	Transform3D targetRightArmTransform = new Transform3D();
		// 	targetRightArmTransform.Origin = grab.toPosition;
		// 	targetRightArmTransform.Basis = rightArmTransform.Basis;
		// 	GD.Print($"target: {targetRightArmTransform}");

		// 	skeleton.SetBonePose(rightLowerArmBoneIndex, targetRightArmTransform);
		// }
		// else
		// {
		// 	skeleton.SetBonePose(leftLowerArmBoneIndex, Transform3D.Identity);
		// 	skeleton.SetBonePose(rightLowerArmBoneIndex, Transform3D.Identity);
		// }
	}
}
