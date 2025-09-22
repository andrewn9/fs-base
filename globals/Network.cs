using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;

public partial class Network : Node
{
    public ServerInfo serverInfo = new();

    [Signal]
    public delegate void ServerCreatedEventHandler();
    
    [Signal]
    public delegate void JoinSuccessEventHandler();
    
    [Signal]
    public delegate void JoinFailEventHandler();

    public Error CreateServer()
    {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(serverInfo.UsedPort, serverInfo.MaxPlayers);
        if (error != Error.Ok)
            return error;

        Multiplayer.MultiplayerPeer = peer;
        EmitSignal(SignalName.ServerCreated);
        EmitSignal(SignalName.JoinSuccess);
        
        return Error.Ok;
    }

    public Error JoinServer(string ip, int port)
    {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(ip, port);
        if (error != Error.Ok)
            return error;

        Multiplayer.MultiplayerPeer = peer;
        return Error.Ok;
    }

    // Peer trying to connect to server is notified on success
    private void _OnConnectedToServer()
    {
        EmitSignal(SignalName.JoinSuccess);
    }

    // Everyone gets notified whenever a new client joins the server
    private void _OnPlayerConnected(long id)
    {
        var gameState = GetNode<GameState>("/root/GameState");
        if (!gameState.Players.ContainsKey(id))
        {
            GD.Print($"new connection: {id} .. pending data");
        }
    }

    // RPC to update a specific player's data
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void _RegisterSelf(Godot.Collections.Dictionary clientInfo)
    {
        var senderId = Multiplayer.GetRemoteSenderId();
        var gameState = GetNode<GameState>("/root/GameState");
        
        gameState.Players[senderId] = clientInfo;
        GD.Print($"registered player: {senderId}: {clientInfo}");
        
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName._UpdatePlayerList, gameState.Players);
            GD.Print("sent updated player list to others");

            Dictionary spawnDef = new CharacterDefinition(true, Vector3.Zero, Vector2.Zero).Serialize();
            Rpc(MethodName._SpawnCharacter, senderId, spawnDef);

            GD.Print("sending existing objects to new connection");
            RpcId(senderId, MethodName._AddExistingObjects, gameState.GameObjects);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Unreliable)]
    public void UpdateDefinition(Dictionary def)
    {
        var senderId = Multiplayer.GetRemoteSenderId();
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.GameObjects[senderId].AsGodotDictionary()["characterDefinition"] = def;
        GetNode<Client>("/root/Client").World.GetNode<Character>(senderId.ToString()).LoadDefinition();
    }


    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public async void _AddExistingObjects(Godot.Collections.Dictionary required)
    {
        var gameState = GetNode<GameState>("/root/GameState");
        var client = GetNode<Client>("/root/Client");

        foreach (var key in required.Keys)
        {
            if (gameState.GameObjects.ContainsKey(key))
                continue;

            var id = key.AsInt64();
            GD.Print($"spawning existing character for {id}");

            Dictionary def = required[key].AsGodotDictionary()["characterDefinition"].AsGodotDictionary();
            client.SpawnCharacter(id, def);

            var gameObjectData = new Godot.Collections.Dictionary
            {
                { "characterDefinition", def }
            };
            gameState.GameObjects[id] = gameObjectData;
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public async void _SpawnCharacter(long id, Dictionary def)
    {
        var client = GetNode<Client>("/root/Client");
        client.SpawnCharacter(id, def);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void _UpdatePlayerList(Godot.Collections.Dictionary allPlayers)
    {
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.Players = allPlayers;
        GD.Print($"received player list: {gameState.Players}");
    }

    // Everyone gets notified whenever someone disconnects from the server
    private void _OnPlayerDisconnected(long id)
    {
        GD.Print($"{id} disconnected from the game");
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.Players.Remove(id);
        Rpc(MethodName._RemovePlayer, id);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void _RemovePlayer(long playerId)
    {
        var gameState = GetNode<GameState>("/root/GameState");
        if (gameState.GameObjects.ContainsKey(playerId))
        {
            gameState.GameObjects.Remove(playerId);
        }
        gameState.Players.Remove(playerId);
    }

    // Peer trying to connect to server is notified on failure
    private void _OnConnectionFailed()
    {
        EmitSignal(SignalName.JoinFail);
        Multiplayer.MultiplayerPeer = null;
    }

    private void _OnDisconnectedFromServer()
    {
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.Players.Clear();
    }

    public override void _Ready()
    {
        Multiplayer.PeerConnected += _OnPlayerConnected;
        Multiplayer.PeerDisconnected += _OnPlayerDisconnected;
        Multiplayer.ConnectedToServer += _OnConnectedToServer;
        Multiplayer.ConnectionFailed += _OnConnectionFailed;
        Multiplayer.ServerDisconnected += _OnDisconnectedFromServer;
    }
}