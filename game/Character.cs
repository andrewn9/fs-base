using Godot;

public partial class Character : Node3D
{
    public CharacterBody3D controller;
    public Skeleton3D skeleton;
    public int headBoneIndex;
    public Movement movement;
    public AnimationPlayer animationPlayer;

    // Input replication struct
    public struct PlayerInput
    {
        public Vector2 MoveDir;
        public Vector3 LookVec;
    }

    private float _rpcTimer = 0f;
    private const float RpcInterval = 1 / 10f;

    private float _lastInputSendTime = 0f;
    public int MaxInputSendsPerSecond = 50;

    // Store latest input for replication
    private Vector2 _latestMoveDir = Vector2.Zero;
    private bool _latestJump = false;
    private Vector3 _latestLookVec = Vector3.Zero;
    private Transform3D _targetTransform;
    private const float InterpSpeed = 10f; // Higher = faster interpolation

    private Client client;

    public override void _Ready()
    {
        controller = GetNode<CharacterBody3D>("CharacterBody3D");
        animationPlayer = GetNode<AnimationPlayer>("CharacterBody3D/model/AnimationPlayer");
        movement = controller as Movement;
        client = GetNode<Client>("/root/Client");
        skeleton = GetNode<Skeleton3D>("CharacterBody3D/model/Skeleton3D");
        headBoneIndex = skeleton.FindBone("Head");

        GD.Print("I'm here");
    }
    // Called by local client to send input to server
    public void SendInput(Vector2 moveDir, Vector3 lookVec)
    {
        _latestMoveDir = moveDir;
        _latestLookVec = lookVec;

        float currentTime = Time.GetTicksMsec() / 1000f;
        float minInterval = 1f / MaxInputSendsPerSecond;

        // Always process local input immediately
        if (Multiplayer.IsServer() || IsMultiplayerAuthority())
        {
            ReceiveInput(moveDir, lookVec);
        }
        if (currentTime - _lastInputSendTime >= minInterval)
        {
            Rpc(nameof(ReceiveInput), moveDir, lookVec);
            _lastInputSendTime = currentTime;
            // GD.Print($"Sent input: move {moveDir}, look {lookVec}");
        }
    }

    // Called by server to receive input
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    public void ReceiveInput(Vector2 moveDir, Vector3 lookVec)
    {
        // GD.Print($"{Multiplayer.GetRemoteSenderId()}Received input: move {moveDir}, look {lookVec}");
        _latestMoveDir = moveDir;
        _latestLookVec = lookVec;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SendJump()
    {
        movement.Jump();
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SnapTo(Transform3D transform)
    {
        controller.GlobalTransform = transform;
        _targetTransform = transform;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void LerpTo(Transform3D transform)
    {
        _targetTransform = transform;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsMultiplayerAuthority() || Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (@event is InputEventMouseMotion mouseMotion)
        {
            movement.Look(mouseMotion.Relative);
        }
    }

    public override void _Process(double delta)
    {
        if (client == null || !client.Loaded)
            return;

        if (movement.Velocity.Length() > 0.1f)
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

        if (!IsMultiplayerAuthority())
        {
            return;
        }

        if (movement != null && movement.controlled)
        {
            var inputDir = Input.GetVector("move_left", "move_right", "move_up", "move_down");
            Vector3 lookVec = movement.LookVector;

            if (!inputDir.IsEqualApprox(_latestMoveDir) || !lookVec.IsEqualApprox(_latestLookVec))
            {
                SendInput(inputDir, lookVec);
            }
            if (movement.canJump && Input.IsActionPressed("jump"))
            {
                Rpc(nameof(SendJump));
            }
        }

        Network network = GetNode<Network>("/root/Network");
        _rpcTimer += (float)delta;

        if (_rpcTimer >= RpcInterval)
        {
            Rpc(nameof(LerpTo), controller.GlobalTransform);
            _rpcTimer = 0f;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var movement = controller as Movement;
        if (movement != null)
        {
            movement.LookVector = _latestLookVec;
            movement.Move(_latestMoveDir, delta);
        }

        // Make head bone follow look direction
        Vector3 neckDir = _latestLookVec % (2*Mathf.Pi);
        neckDir.X = _latestLookVec.X;
        neckDir.X *= -1;
        neckDir.X = Mathf.Clamp(neckDir.X, -Mathf.Pi / 4, Mathf.Pi / 2);
        neckDir.Y = Mathf.Clamp(neckDir.Y, -Mathf.Pi / 8, Mathf.Pi / 8);

        var headRotation = new Basis(Quaternion.FromEuler(neckDir));
        skeleton.SetBonePose(headBoneIndex, new Transform3D(headRotation, skeleton.GetBonePose(headBoneIndex).Origin));

        // Smoothly interpolate to authoritative transform
        if (!IsMultiplayerAuthority())
        {
            // Interpolate position
            var currentPos = controller.GlobalTransform.Origin;
            var targetPos = _targetTransform.Origin;
            var newPos = currentPos.Lerp(targetPos, (float)delta * InterpSpeed);

            // Interpolate rotation (basis)
            Quaternion currentRot = new Quaternion(controller.GlobalTransform.Basis).Normalized();
            Quaternion targetRot = new Quaternion(_targetTransform.Basis).Normalized();

            Quaternion interpolatedRot = currentRot.Slerp(targetRot, (float)delta * InterpSpeed);
            Basis newBasis = new Basis(interpolatedRot);
            
            controller.GlobalTransform = new Transform3D(newBasis, newPos);
        }
    }
}