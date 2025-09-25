using Godot;
using Godot.Collections;
public partial class Client : Node
{
    public Server server;
    public ClientInfo clientInfo = new();
    public bool Loaded = false;
	public MultiplayerSpawner multiplayerSpawner;

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

        var serverScene = GD.Load<PackedScene>("res://game/server.tscn");
        var tree = GetTree();

        tree.ChangeSceneToPacked(serverScene);
        await ToSignal(tree, SceneTree.SignalName.SceneChanged);
        Node root = tree.CurrentScene;
        server = root as Server;

        Loaded = true;
    }

    public override void _Process(double delta)
    {
        if (!Loaded)
            return;

        var gameState = GetNode<GameState>("/root/GameState");
        // var label = GetNode<Label>("/root/CanvasLayer/Label");

        // label.Text = clientInfo.Name + ".." + Multiplayer.GetUniqueId().ToString();
        // foreach (var entry in gameState.Players)
        // {
        //     label.Text += "\n" + entry.Key + ": " + entry.Value;
        // }

        // label.Text += "\n";

        // foreach (var entry in gameState.GameObjects)
        // {
        //     label.Text += "\n" + entry.Key + ": " + entry.Value;
        // }
    }
}