using Godot;

public partial class Movement : CharacterBody3D
{
    private Camera3D camera;
    private const float Speed = 5.0f;
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

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        var client = GetNode<Client>("/root/Client");
        var parentNode = GetParent<Character>();
        var clientId = client.clientInfo.Id;
        controlled = parentNode.Definition.objectId == clientId;
        camera.Current = controlled;
        LookVector = new Vector3(camera.Rotation.X, Rotation.Y, 0);
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
        canJump = IsOnFloor();
    }
}