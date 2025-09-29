using Godot;
using Godot.Collections;
using System;

public partial class Server : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public bool loaded = false;
	public MultiplayerSpawner MapSpawner;
	public MultiplayerSpawner CharacterSpawner;
	private Node world;
	public override void _Ready()
	{
		MapSpawner = GetNode<MultiplayerSpawner>("MapSpawner");
		CharacterSpawner = GetNode<MultiplayerSpawner>("CharacterSpawner");
		MapSpawner.SpawnFunction = new Callable(this, nameof(SpawnWorld));
		// MultiplayerSpawner expects a function with (int peer, Variant customData)
		CharacterSpawner.SpawnFunction = Callable.From((Variant customData) => SpawnCharacter(customData));

		if (Multiplayer.IsServer())
		{
			MapSpawner.Spawn(1);
		}
		loaded = true;
	}

	private Node SpawnWorld(int peer)
	{
		GD.Print($"[SpawnWorld] Called with peer ID: {peer}");

		var worldScene = GD.Load<PackedScene>("res://game/World.tscn");
		if (worldScene == null)
		{
			GD.PrintErr("[SpawnWorld] Failed to load world scene!");
			return null;
		}

		var world = worldScene.Instantiate() as World;
		if (world == null)
		{
			GD.PrintErr("[SpawnWorld] World scene root is not of type World!");
			return null;
		}

		world.SetMultiplayerAuthority(1);
		this.world = world;

		return world;
	}

	private Node SpawnCharacter(Variant customData)
	{
		var characterScene = GD.Load<PackedScene>("res://game/entities/character.tscn");
		if (characterScene == null)
		{
			GD.PrintErr("Failed to load character scene!");
			return null;
		}

		var character = characterScene.Instantiate() as Character;
		if (character == null)
		{
			GD.PrintErr("Character scene root is not of type Character!");
			return null;
		}

		int peer = Multiplayer.GetRemoteSenderId();
		var label = character.GetNode<Node3D>("CharacterBody3D").GetNode<Label3D>("Label3D");
		if (customData.VariantType == Variant.Type.Dictionary)
		{
			var clientInfo = (Dictionary)customData;
			if (clientInfo.ContainsKey("name"))
				label.Text = clientInfo["name"].AsString();
				
			if (clientInfo.ContainsKey("id"))
				peer = clientInfo["id"].AsInt32();
		}

	character.Name = "Player" + peer;
	character.SetMultiplayerAuthority(peer);
	character.GetNode<Node3D>("CharacterBody3D").SetMultiplayerAuthority(peer);

	foreach (var child in character.GetNode<Node3D>("CharacterBody3D").GetChildren())
	{
		if  (!(child is Node3D))
			continue;
		
		child.SetMultiplayerAuthority(peer);
	}

	Node spawns = world.GetNode<Node>("Spawns");
	Node3D spawnPoint = spawns.GetChild<Node3D>((int)(GD.Randi() % spawns.GetChildCount()));
	character.spawnPosition = spawnPoint.GlobalPosition;
	GD.Print("Spawning character for peer ", peer);
		return character;
	}

	private bool a;
    public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.IsActionJustPressed("quit"))
		{
			a = !a;
			if (a)
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
		}
	}
}
