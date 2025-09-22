using Godot;
using Godot.Collections;
using System;

public partial class Character : Node3D
{
	public long Id;
    public CharacterDefinition def = new();
    public CharacterBody3D controller;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        controller = GetNode<CharacterBody3D>("CharacterBody3D");
    }
    private float _rpcTimer = 0f; // Accumulator for timing
    private const float RpcInterval = 1/20f; // Interval in seconds

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public void LoadDefinition()
	{
		if (Id == Multiplayer.GetUniqueId()) {
            return;
        }
        GameState gameState = GetNode<GameState>("/root/GameState");
        Vector3 pos = (gameState.GameObjects[Id].AsGodotDictionary()["characterDefinition"].AsGodotDictionary()["position"].AsVector3());
        // CharacterDefinition goal = CharacterDefinition.Deserialize(gameState.GameObjects["id"].AsGodotDictionary()["characterDefinition"].AsGodotDictionary());
        def.position = pos;
        controller.GlobalPosition = def.position;
    }
	public override void _Process(double delta)
	{
		if (Id != Multiplayer.GetUniqueId()) {
            return;
        }
		Network network = GetNode<Network>("/root/Network");
        // GD.Print(controller.GlobalPosition);
        def.position = controller.GlobalPosition;
        _rpcTimer += (float)delta;

        // Only send the RPC every 5 seconds
        if (_rpcTimer >= RpcInterval)
        {
            GD.Print("Sent update");
            network.Rpc("UpdateDefinition", def.Serialize());
            _rpcTimer = 0f;
        }

    }
}
