using Godot;

public partial class Movement : CharacterBody3D
{
    private Camera3D camera;
    private const float Speed = 3.0f;
    private const float JumpVelocity = 4.5f;
    public bool controlled = false;
    public bool canJump = false;
    private Vector3 _lookVector = Vector3.Zero;
    public Vector3 LookVector
    {
        get => _lookVector;
        set
        {
            _lookVector = value;
            // Clamp pitch
            _lookVector.X = Mathf.Clamp(_lookVector.X, -Mathf.Pi / 2, Mathf.Pi / 2);
            // Apply rotations
            Rotation = new Vector3(Rotation.X, _lookVector.Y, Rotation.Z);
            camera.Rotation = new Vector3(_lookVector.X, camera.Rotation.Y, camera.Rotation.Z);
        }
    }

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
        var client = GetNode<Client>("/root/Client");
        var parentNode = GetParent<Character>();
        var clientId = client.clientInfo.Id;
        controlled = parentNode.Definition.ObjectId == clientId;
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
        MoveAndSlide();
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
    }
}