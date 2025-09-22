using Godot;

public partial class Movement : CharacterBody3D
{
    private Camera3D camera;

    private const float Speed = 5.0f;
    private const float JumpVelocity = 4.5f;

    private bool controlled = false;

    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera3D");
        
        var client = GetNode<Client>("/root/Client");
        var parentNode = GetParent<Character>();
        var parentId = parentNode.Id;
        var clientId = client.clientInfo.Id;
        
        controlled = parentId == clientId;
        
        if (controlled)
        {
            camera.ProcessMode = Node.ProcessModeEnum.Disabled;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!controlled || Input.MouseMode != Input.MouseModeEnum.Captured) return;
        
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * 0.005f);
            camera.RotateX(-mouseMotion.Relative.Y * 0.005f);
            camera.Rotation = new Vector3(
                Mathf.Clamp(camera.Rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2),
                camera.Rotation.Y,
                camera.Rotation.Z
            );
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Add the gravity
        if (!IsOnFloor())
        {
            Velocity += GetGravity() * (float)delta;
        }

        // Handle jump
        if (controlled)
        {
            if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            {
                Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);
            }

            var inputDir = Input.GetVector("left", "right", "up", "down");
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

        MoveAndSlide();
    }
}