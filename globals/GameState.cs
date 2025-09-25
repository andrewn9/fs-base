using Godot;
using Godot.Collections;

public partial class GameState : Node
{
    public Dictionary Players { get; set; } = new();
    // public Dictionary<long, Dictionary> GameObjects { get; set; } = new();
    // public Dictionary<long, Net.Object> GameObjectRef { get; set; } = new();
}