using Godot;

public partial class Movement : CharacterBody3D
{
    private Camera3D camera;
    private const float Speed = 3.0f;
    private const float JumpVelocity = 2.5f;
    public bool controlled = false;
    public bool canJump = false;
    public bool isGrounded => IsOnFloor();
    private Vector3 _lookVector = Vector3.Zero;
    [Export]
    public float camXMin = -Mathf.Pi / 3;
    [Export]
    public float camXMax = Mathf.Pi / 2;
    public Vector3 LookVector
    {
        get => _lookVector;
        set
        {
            _lookVector = value;
            _lookVector.X = Mathf.Clamp(_lookVector.X,  camXMin, camXMax);
            Rotation = new Vector3(Rotation.X, _lookVector.Y, Rotation.Z);
            camera.Rotation = new Vector3(_lookVector.X, camera.Rotation.Y, camera.Rotation.Z);
        }
    }
	[Export] public float Mass = 80.0f;

	[Export] public float PushForceScalar = 5.0f;

    // Camera bob fields
    [Export] public float CameraBobAmplitude { get; set; } = 0.038f;
    [Export] public float CameraBobFrequency { get; set; } = 11.565f;
    [Export] public float CameraBobSideAmplitude { get; set; } = 0.03f;
    [Export] public float CameraBobSpeedThreshold { get; set; } = 0.1f;
    [Export] public float CameraBobJumpAmplitude { get; set; } = 0.12f;
    [Export] public float CameraBobJumpDecay { get; set; } = 8.0f;  
    [Export] public float CameraBobLandAmplitude { get; set; } = -0.09f;
    [Export] public float CameraBobLandDecay { get; set; } = 50.0f;
    private float _bobTimer = 0f;
    private float _jumpBobOffset = 0f;
    private float _landBobOffset = 0f;
    private bool _wasOnFloor = true;
    private Vector3 _cameraDefaultPosition = Vector3.Zero;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        controlled = IsMultiplayerAuthority();
        camera.Current = controlled;
        LookVector = new Vector3(camera.Rotation.X, Rotation.Y, 0);

        // Store default camera position for bobbing
        _cameraDefaultPosition = camera.Position;
    }

    public void Look(Vector2 mouseDelta)
    {
        LookVector = new Vector3(
            _lookVector.X - mouseDelta.Y * 0.005f, // Pitch
            _lookVector.Y - mouseDelta.X * 0.005f, // Yaw
            0
        );
    }

    public void Jump()
    {
        Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
        canJump = false;
        // Add jump bob effect
        _jumpBobOffset = CameraBobJumpAmplitude;
    }

    public void Move(Vector2 inputDir, double delta)
    {
        // Add gravity
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }

        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            Velocity = new Vector3(
                direction.X * Speed,
                Velocity.Y,
                direction.Z * Speed
            );
        }
        else
        {
            Velocity = new Vector3(
                Mathf.MoveToward(Velocity.X, 0, Speed),
                Velocity.Y,
                Mathf.MoveToward(Velocity.Z, 0, Speed)
            );
        }
    }
    
	private void PushAwayRigidBodies()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D CollisionData = GetSlideCollision(i);

            GodotObject UnkObj = CollisionData.GetCollider();

            if (UnkObj is RigidBody3D)
            {
                RigidBody3D Obj = UnkObj as RigidBody3D;

                // Objects with more mass than us should be harder to push.
                // But doesn't really make sense to push faster than we are going
                float MassRatio = Mathf.Min(1.0f, Mass / Obj.Mass);

                // Optional add: Don't push object at all if it's 4x heavier or more
                if (MassRatio < 0.25f) continue;

                Vector3 PushDir = -CollisionData.GetNormal();

                // How much velocity the object needs to increase to match player velocity in the push direction
                float VelocityDiffInPushDir = Velocity.Dot(PushDir) - Obj.LinearVelocity.Dot(PushDir);

                // Only count velocity towards push dir, away from character
                VelocityDiffInPushDir = Mathf.Max(0.0f, VelocityDiffInPushDir);

                PushDir.Y = 0; // Don't push object from above/below

                float PushForce = MassRatio * PushForceScalar;
                Obj.ApplyImpulse(PushDir * VelocityDiffInPushDir * PushForce, CollisionData.GetPosition() - Obj.GlobalPosition);
            }
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        bool isOnFloor = IsOnFloor();
        canJump = isOnFloor;

        // Detect landing
        if (!_wasOnFloor && isOnFloor)
        {
            _landBobOffset = CameraBobLandAmplitude;
        }
        _wasOnFloor = isOnFloor;

        // Dynamic camera bob logic
        float speed = new Vector2(Velocity.X, Velocity.Z).Length();
        float dynamicAmplitude = Mathf.Lerp(CameraBobAmplitude, CameraBobAmplitude * 2f, Mathf.Clamp(speed / Speed, 0, 1));
        float dynamicFrequency = Mathf.Lerp(CameraBobFrequency, CameraBobFrequency * 1.5f, Mathf.Clamp(speed / Speed, 0, 1));

        float totalBobOffset = _jumpBobOffset + _landBobOffset;

        if (controlled && speed > CameraBobSpeedThreshold && isOnFloor)
        {
            _bobTimer += (float)delta * dynamicFrequency;
            float bobOffset = Mathf.Sin(_bobTimer) * dynamicAmplitude;
            float sideOffset = Mathf.Sin(_bobTimer * 0.5f) * CameraBobSideAmplitude * Mathf.Sign(Velocity.X);
            Vector3 targetPos = _cameraDefaultPosition + new Vector3(sideOffset, bobOffset + totalBobOffset, 0);
            camera.Position = camera.Position.Lerp(targetPos, 0.15f);
        }
        else
        {
            // Reset bob timer when not moving
            _bobTimer = 0f;
            Vector3 targetPos = _cameraDefaultPosition + new Vector3(0, totalBobOffset, 0);
            camera.Position = camera.Position.Lerp(targetPos, 0.15f);
        }

        // Decay jump bob offset
        if (_jumpBobOffset != 0f)
        {
            _jumpBobOffset = Mathf.MoveToward(_jumpBobOffset, 0f, CameraBobJumpDecay * (float)delta);
        }
        // Decay land bob offset
        if (_landBobOffset != 0f)
        {
            _landBobOffset = Mathf.MoveToward(_landBobOffset, 0f, CameraBobLandDecay * (float)delta);
        }
        PushAwayRigidBodies();
        MoveAndSlide();
    }
}