using Godot;

public partial class Character : Object
{
    public CharacterBody3D controller;

    public override void _Ready()
    {
        controller = GetNode<CharacterBody3D>("CharacterBody3D");
        var client = GetNode<Client>("/root/Client");
        if (Definition != null && Definition.objectId == client.clientInfo.Id)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }
    private float _rpcTimer = 0f;
    private const float RpcInterval = 1f;
    
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
    public override void _Process(double delta)
    {
        if (Definition.objectId != Multiplayer.GetUniqueId())
        {
            return;
        }
        UpdateDefinition();

        Network network = GetNode<Network>("/root/Network");
        _rpcTimer += (float)delta;

        if (_rpcTimer >= RpcInterval)
        {
            // GD.Print("Sent update");
            network.Rpc("UpdateDefinition", Definition.objectId, Definition.Serialize());
            _rpcTimer = 0f;
        }
    }
}
