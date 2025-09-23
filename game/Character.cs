using Godot;

public partial class Character : Object
{
    public CharacterBody3D controller;
    public Movement movement;

    // Input replication struct
    public struct PlayerInput
    {
        public Vector2 MoveDir;
        public Vector3 LookVec;
    }

    public override void _Ready()
    {
        controller = GetNode<CharacterBody3D>("CharacterBody3D");
        movement = controller as Movement;
        var client = GetNode<Client>("/root/Client");
        if (Definition != null && Definition.objectId == client.clientInfo.Id)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }
    private float _rpcTimer = 0f;
    private const float RpcInterval = 10f;

    // Called by local client to send input to server
    public void SendInput(Vector2 moveDir, Vector3 lookVec)
    {
        Rpc(nameof(ReceiveInput), moveDir, lookVec);
        GD.Print($"Sent input: move {moveDir}, look {lookVec}");
    }

    // Store latest input for replication
    private Vector2 _latestMoveDir = Vector2.Zero;
    private bool _latestJump = false;
    private Vector3 _latestLookVec = Vector3.Zero;

    // Called by server to receive input
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.UnreliableOrdered)]
    public void ReceiveInput(Vector2 moveDir, Vector3 lookVec)
    {
        GD.Print($"{Multiplayer.GetRemoteSenderId()}Received input: move {moveDir}, look {lookVec}");
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
            controller.GlobalTransform = Definition.Transform;
        }
    }

    public override void UpdateDefinition()
    {
        if (Definition != null)
        {
            Definition.Transform = controller.GlobalTransform;
        }
        GameState gameState = GetNode<GameState>("/root/GameState");
        gameState.GameObjects[Definition.objectId] = Definition.Serialize();
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
        if (Definition.objectId != Multiplayer.GetUniqueId())
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
            network.Rpc(nameof(UpdateDefinition), Definition.objectId, Definition.Serialize());
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
    }
}
