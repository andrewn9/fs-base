using Godot;
using Godot.Collections;
public partial class Client : Node
{
    public ClientInfo clientInfo = new();
    public Node World;
    public bool Loaded = false;

    public override void _Ready()
    {
        DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Mailbox);

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

    public async void SpawnObject(long id, Dictionary def)
    {
        GameState gameState = GetNode<GameState>("/root/GameState");
        ObjectDefinition objDef = ObjectDefinition.Deserialize(def);
        // while (!loaded)
        //     await GetTree().ProcessFrame();
        //
        // var character = GD.Load<PackedScene>("res://game/character.tscv");
        // var inst = character.Instantiate();
        // world.AddChild(inst);
        // Currently empty - pass
        if (objDef.ObjectType == Globals.Classes.ObjectType.Player)
        {
            GD.Print($"spawning character {id}");

            var characterScene = GD.Load<PackedScene>("res://game/character.tscn");
            while (!Loaded)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }

            var inst = characterScene.Instantiate<Node3D>() as Character;
            inst.Definition = objDef;
            inst.Name = id.ToString();
            World.AddChild(inst);
            inst.SnapTo(objDef.Transform);

            gameState.GameObjects[id] = inst.Definition.Serialize();
            gameState.GameObjectRef[id] = inst;
        }
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