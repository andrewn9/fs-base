using Godot;
[Tool]
public partial class IkController : Node
{
	[Export]
    public Skeleton3D skeleton { get; set; }
	[Export]
    public int headBoneIndex { get; set; }
	public Vector3 headLookVector { get; set; }
	[Export]
	public LookAtModifier3D LeftArmLookAt { get; set; }
	[Export]
	public LookAtModifier3D RightArmLookAt { get; set; }
	[Export]
	public Marker3D LeftHandMarker { get; set; }
	[Export]
	public Marker3D RightHandMarker { get; set; }

	[Export]
	public float ArmLength { get; set; } = 3f;
	private MultiplayerSynchronizer sync;
	public float restLength { get; private set; }
	
	[Export]
	public int upperArmR = -1;
	[Export]
	public int handR = -1;
	[Export]
	public int upperArmL = -1;
	[Export]
	public int handL = -1;
	public override void _Ready()
	{
		sync = GetNode<MultiplayerSynchronizer>("./MultiplayerSynchronizer");
		headBoneIndex = skeleton.FindBone("Head");
		upperArmR = skeleton.FindBone("UpperArm.R");
		handR = skeleton.FindBone("LowerArm.R");
		upperArmL = skeleton.FindBone("UpperArm.L");
		handL = skeleton.FindBone("LowerArm.L");
		Vector3 upperArmRest = skeleton.GetBoneRest(upperArmR).Origin;
		Vector3 handRest = skeleton.GetBoneRest(handR).Origin;
		restLength = (handRest - upperArmRest).Length();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public void Grab(string leftright, Vector3 position)
	{
		if (leftright == "left")
		{
			LeftArmLookAt.Active = true;
			LeftHandMarker.GlobalPosition = position;
		}
		else
		{
			RightArmLookAt.Active = true;
			RightHandMarker.GlobalPosition = position;
		}
	}
	public void Release(string leftright)
	{
		if (leftright == "left")
		{
			LeftArmLookAt.Active = false;
			skeleton.SetBonePoseScale(upperArmL, new Vector3(1, 1, 1));
		}
		else
		{
			RightArmLookAt.Active = false;
			skeleton.SetBonePoseScale(upperArmR, new Vector3(1, 1, 1));
		}
	}


	public override void _Process(double delta)
	{
		if (skeleton == null)
		{
			GD.Print("no skeleton");
			return;
		}
		Vector3 neckDir = headLookVector;
		neckDir.X *= -1;
		neckDir.X = Mathf.Clamp(neckDir.X, -Mathf.Pi / 4, Mathf.Pi / 2);
		neckDir.Y = Mathf.Clamp(neckDir.Y, -Mathf.Pi / 8, Mathf.Pi / 8);

		var headRotation = new Basis(Quaternion.FromEuler(neckDir));
		skeleton.SetBonePose(headBoneIndex, new Transform3D(headRotation, skeleton.GetBonePose(headBoneIndex).Origin));

		if (RightArmLookAt.Active)
		{
			// Get current world positions
			Vector3 upperArmWorld = skeleton.ToGlobal(skeleton.GetBoneGlobalPose(upperArmR).Origin);
			Vector3 handTargetWorld = RightHandMarker.GlobalPosition;
			float targetLength = (handTargetWorld - upperArmWorld).Length();

			// Calculate scale factor
			float scale = ArmLength * targetLength / restLength;

			skeleton.SetBonePoseScale(upperArmR, new Vector3(1, scale, 1));
		}
		if (LeftArmLookAt.Active)
		{
			// Get current world positions
			Vector3 upperArmWorld = skeleton.ToGlobal(skeleton.GetBoneGlobalPose(upperArmL).Origin);
			Vector3 handTargetWorld = LeftHandMarker.GlobalPosition;
			float targetLength = (handTargetWorld - upperArmWorld).Length();

			// Calculate scale factor
			float scale = ArmLength * targetLength / restLength;

			skeleton.SetBonePoseScale(upperArmL, new Vector3(1, scale, 1));
		}
	}
}
