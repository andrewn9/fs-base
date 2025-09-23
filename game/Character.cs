using Godot;

public partial class Character : Object
{
    public CharacterBody3D controller;
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

    public override void _Ready()
    {
        controller = GetNode<CharacterBody3D>("CharacterBody3D");
        animationPlayer = GetNode<AnimationPlayer>("CharacterBody3D/model/AnimationPlayer");
        movement = controller as Movement;
        var client = GetNode<Client>("/root/Client");
        if (Definition != null && Definition.ObjectId == client.clientInfo.Id)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }
    // Called by local client to send input to server
    public void SendInput(Vector2 moveDir, Vector3 lookVec)
    {
        _latestMoveDir = moveDir;
        _latestLookVec = lookVec;

        float currentTime = Time.GetTicksMsec() / 1000f;
        float minInterval = 1f / MaxInputSendsPerSecond;

        // Always process local input immediately
        if (Definition != null && Definition.ObjectId == Multiplayer.GetUniqueId())
        {
            ReceiveInput(moveDir, lookVec);
        }
        if (currentTime - _lastInputSendTime >= minInterval)
        {
            Rpc(nameof(ReceiveInput), moveDir, lookVec);
            _lastInputSendTime = currentTime;
            GD.Print($"Sent input: move {moveDir}, look {lookVec}");
        }
    }

    // Called by server to receive input
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    public void ReceiveInput(Vector2 moveDir, Vector3 lookVec)
    {
        // GD.Print($"{Multiplayer.GetRemoteSenderId()}Received input: move {moveDir}, look {lookVec}");
        _latestMoveDir = moveDir;
        _latestLookVec = lookVec;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void SendJump()
    {
        movement.Jump();
    }

    public override void LoadDefinition()
    {
        if (Definition != null)
        {
            _targetTransform = Definition.Transform;
        }
    }

    public override void UpdateDefinition()
    {
        if (Definition != null)
        {
            Definition.Transform = controller.GlobalTransform;
        }
        GameState gameState = GetNode<GameState>("/root/GameState");
        gameState.GameObjects[Definition.ObjectId] = Definition.Serialize();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!movement.controlled || Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (@event is InputEventMouseMotion mouseMotion)
        {
            movement.Look(mouseMotion.Relative);
        }
    }

    public override void _Process(double delta)
    {
        if ((movement).Velocity.Length() > 0.1f)
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


        if (Definition.ObjectId != Multiplayer.GetUniqueId())
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

        UpdateDefinition();

        Network network = GetNode<Network>("/root/Network");
        _rpcTimer += (float)delta;

        if (_rpcTimer >= RpcInterval)
        {
            // GD.Print("Sent update");
            network.Rpc(nameof(UpdateDefinition), Definition.ObjectId, Definition.Serialize());
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

        // Smoothly interpolate to authoritative transform
        if (!movement.controlled)
        {
            // Interpolate position
            var currentPos = controller.GlobalTransform.Origin;
            var targetPos = _targetTransform.Origin;
            var newPos = currentPos.Lerp(targetPos, (float)delta * InterpSpeed);

            // Interpolate rotation (basis)
            var currentBasis = controller.GlobalTransform.Basis;
            var targetBasis = _targetTransform.Basis;
            var newBasis = currentBasis.Slerp(targetBasis, (float)delta * InterpSpeed);

            controller.GlobalTransform = new Transform3D(newBasis, newPos);
        }
    }
}