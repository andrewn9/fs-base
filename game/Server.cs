using Godot;
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
		CharacterSpawner.SpawnFunction = new Callable(this, nameof(SpawnCharacter));

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

	private Node SpawnCharacter(int peer)
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

		character.Name = "Player" + peer;
		character.SetMultiplayerAuthority(peer);
		character.GetNode<Node3D>("CharacterBody3D").SetMultiplayerAuthority(peer);

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
