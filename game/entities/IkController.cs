using Godot;
[Tool]
public partial class IkController : Node
{
	[Export]
    public Skeleton3D skeleton { get; set; }
	[Export]
    public int headBoneIndex { get; set; }
	[Export]
    public int lowerLeftArmBoneIndex { get; set; }
	[Export]
    public int lowerRightArmBoneIndex { get; set; }
	[Export]
    public int upperLeftArmBoneIndex { get; set; }
	[Export]
    public int upperRightArmBoneIndex { get; set; }
	[Export]
	public Vector3 headLookVector { get; set; }
	[Export]
	public Node3D handTarget { get; set; }

	[Export]
	public Vector3 handTargetPosition { get; set; }
	[Export]
	public Vector3 skeletonLocalHandTargetPosition { get; set; }
	// Called when the node enters the scene tree for the first time.

	[Export]
	public Vector3 handOffset { get; set; } = Vector3.Zero;

	[Export]
	Transform3D finalTransform = Transform3D.Identity;

	[Export]
	public bool active;

	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (skeleton == null)
			return;
		if (Engine.IsEditorHint())
		{
			headBoneIndex = skeleton.FindBone("Head");
			lowerLeftArmBoneIndex = skeleton.FindBone("LowerArm.L");
			lowerRightArmBoneIndex = skeleton.FindBone("LowerArm.R");
			upperLeftArmBoneIndex = skeleton.FindBone("UpperArm.L");
			upperRightArmBoneIndex = skeleton.FindBone("UpperArm.R");
		}
		Vector3 neckDir = headLookVector;
		neckDir.X *= -1;
		neckDir.X = Mathf.Clamp(neckDir.X, -Mathf.Pi / 4, Mathf.Pi / 2);
		neckDir.Y = Mathf.Clamp(neckDir.Y, -Mathf.Pi / 8, Mathf.Pi / 8);

		var headRotation = new Basis(Quaternion.FromEuler(neckDir));
		skeleton.SetBonePose(headBoneIndex, new Transform3D(headRotation, skeleton.GetBonePose(headBoneIndex).Origin));

		if (handTarget != null && active)
		{
			handTargetPosition = handTarget.GlobalPosition;
			skeletonLocalHandTargetPosition = skeleton.ToLocal(handTargetPosition);

			Transform3D boneGlobalRest = skeleton.GetBoneGlobalRest(lowerRightArmBoneIndex);
			Transform3D skeletonGlobal = skeleton.GlobalTransform;

			// Convert target global position to skeleton local space
			Vector3 targetInSkeletonSpace = skeletonGlobal.Basis.Inverse() * (handTargetPosition - skeletonGlobal.Origin);
			Vector3 targetInBonePoseSpace = boneGlobalRest.Basis.Inverse() * (targetInSkeletonSpace - boneGlobalRest.Origin);


			Quaternion targetRotation = Quaternion.FromEuler(handTarget.Rotation);

			skeleton.SetBonePosePosition(lowerRightArmBoneIndex, targetInBonePoseSpace);
			skeleton.SetBonePoseRotation(lowerRightArmBoneIndex, targetRotation);
		}
		else
		{
			skeleton.ResetBonePose(lowerRightArmBoneIndex);
		}
	}
}
