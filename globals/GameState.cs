using Godot;
using Godot.Collections;

public partial class GameState : Node
{
    public Dictionary Players { get; set; } = new Dictionary();
    public Dictionary GameObjects { get; set; } = new Dictionary();
}