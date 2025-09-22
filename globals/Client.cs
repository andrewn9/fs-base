using Godot;
using Godot.Collections;
public partial class Client : Node
{
    public ClientInfo clientInfo = new();
    public Node World;
    public bool Loaded = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var network = GetNode<Network>("/root/Network");
        network.JoinSuccess += _OnJoin;
    }

    private async void _OnJoin()
    {
        GD.Print("Connected. Sending required data");
        clientInfo.Id = Multiplayer.GetUniqueId();

        var network = GetNode<Network>("/root/Network");
        network.Rpc(Network.MethodName._RegisterSelf, clientInfo.Serialize());

        var worldScene = GD.Load<PackedScene>("res://game/world.tscn");
        var tree = GetTree();
        
        tree.ChangeSceneToPacked(worldScene);
        await ToSignal(tree, SceneTree.SignalName.SceneChanged);
        
        World = tree.CurrentScene;
        Loaded = true;
    }

    public async void SpawnCharacter(long id, Dictionary def)
    {
        CharacterDefinition characterDefinition = CharacterDefinition.Deserialize(def);
        // while (!loaded)
        //     await GetTree().ProcessFrame();
        //
        // var character = GD.Load<PackedScene>("res://game/character.tscv");
        // var inst = character.Instantiate();
        // world.AddChild(inst);
        // Currently empty - pass
        GD.Print($"spawning character for {id}");

        var gameState = GetNode<GameState>("/root/GameState");
        var characterScene = GD.Load<PackedScene>("res://game/character.tscn");

        while (!Loaded)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
        
        var inst = characterScene.Instantiate<Node3D>();
        inst.Set("Id", id);
        inst.Name = id.ToString();
        World.AddChild(inst);

        if (characterDefinition.position == Vector3.Zero)
        {
            characterDefinition.position = World.GetNode<Node3D>("spawn").GlobalPosition;
        }

        inst.GetNode<CharacterBody3D>("CharacterBody3D").GlobalPosition = characterDefinition.position;
        
        var gameObjectData = new Dictionary
        {
            { "characterDefinition", def }
        };
        gameState.GameObjects[id] = gameObjectData;
    }

    public override void _Process(double delta)
    {
        if (!Loaded)
            return;

        var gameState = GetNode<GameState>("/root/GameState");
        var label = World.GetNode<Label>("CanvasLayer/Label");
        
        label.Text = clientInfo.Name + ".." + Multiplayer.GetUniqueId().ToString();
        foreach (var entry in gameState.Players)
        {
            label.Text += "\n" + entry.Key + ": " + entry.Value;
        }

        label.Text += "\n";

        foreach (var entry in gameState.GameObjects)
        {
            label.Text += "\n" + entry.Key + ": " + entry.Value;
        }
    }
}