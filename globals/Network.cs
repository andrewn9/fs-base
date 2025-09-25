using Godot;
using Godot.Collections;
using System;
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

    private void _OnConnectedToServer()
    {
        EmitSignal(SignalName.JoinSuccess);
    }

    private void _OnPlayerConnected(long id)
    {
        GD.Print($"new connection: {id}");
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public async void _RegisterSelf(Dictionary clientInfo)
    {
        var senderId = Multiplayer.GetRemoteSenderId();
        GameState gameState = GetNode<GameState>("/root/GameState");
        Client client = GetNode<Client>("/root/Client");

        gameState.Players[senderId] = clientInfo;
        GD.Print($"registered player: {senderId}: {clientInfo}");

        if (Multiplayer.IsServer())
        {
            Rpc(MethodName._UpdatePlayerList, gameState.Players);

            while (!client.Loaded || !client.server.loaded)
            {
                GD.Print("waiting for host world to be ready...");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
            }
            client.server.CharacterSpawner.Spawn(senderId);
        }
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void _UpdatePlayerList(Godot.Collections.Dictionary allPlayers)
    {
        var gameState = GetNode<GameState>("/root/GameState");
        gameState.Players = allPlayers;
    }

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
        gameState.Players.Remove(playerId);
    }

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