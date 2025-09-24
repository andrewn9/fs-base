using Godot;
using System;
public partial class Item : Net.Object
{
    public override void _Ready()
    {
        GameState gameState = GetNode<GameState>("/root/GameState");
        Client client = GetNode<Client>("/root/Client");

        if (Definition == null && Multiplayer.IsServer())
        {
            Definition = new ObjectDefinition();
            Definition.ObjectType = Globals.Classes.ObjectType.Item;
            Definition.ObjectId = client.GetUniqueId();
            Definition.Transform = GetNode<RigidBody3D>("RigidBody3D").GlobalTransform;
            Definition.SceneName = "box.tscn";
            GD.Print($"Item definition ready with id {Definition.ObjectId}");
            LoadDefinition();
            gameState.GameObjects[Definition.ObjectId] = Definition.Serialize();
            gameState.GameObjectRef[Definition.ObjectId] = this;
        }
        else if (Definition == null)
        {
            QueueFree();
            return;
        }
    }

    public override void LoadDefinition()
    {
        if (Definition != null)
        {
            GetNode<RigidBody3D>("RigidBody3D").GlobalTransform = Definition.Transform;
        }
    }

    public override void UpdateDefinition()
    {
        Definition.Transform = GetNode<RigidBody3D>("RigidBody3D").GlobalTransform;
        GameState gameState = GetNode<GameState>("/root/GameState");
        gameState.GameObjects[Definition.ObjectId] = Definition.Serialize();
        // Network network = GetNode<Network>("/root/Network");
        // network.Rpc(nameof(Network.UpdateDefinition), Definition.ObjectId, Definition.Serialize());
    }
    public void _on_rigid_body_3d_sleeping_state_changed(bool sleeping)
    {
        if (Multiplayer.IsServer())
        {
            GD.Print("yo");
            UpdateDefinition();
            Network network = GetNode<Network>("/root/Network");
            network.Rpc(nameof(Network.UpdateDefinition), Definition.ObjectId, Definition.Serialize());
        }
    }

    public void _on_box_property_list_changed()
    {
        if (Multiplayer.IsServer())
        {
            UpdateDefinition();
            Network network = GetNode<Network>("/root/Network");
            network.Rpc(nameof(Network.UpdateDefinition), Definition.ObjectId, Definition.Serialize());
        }
    }

}
