using Godot;
using Godot.Collections;

[System.Serializable]
public class ServerInfo
{
    public string Name { get; set; } = "Server";
    public int MaxPlayers { get; set; } = 0;
    public int UsedPort { get; set; } = 0;

    public ServerInfo()
    {
    }

    public ServerInfo(string name, int maxPlayers, int usedPort)
    {
        Name = name;
        MaxPlayers = maxPlayers;
        UsedPort = usedPort;
    }

    // Convert to Dictionary for network transmission
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            { "name", Name },
            { "max_players", MaxPlayers },
            { "used_port", UsedPort }
        };
    }

    // Create from Dictionary received from network
    public static ServerInfo Deserialize(Dictionary dict)
    {
        return new ServerInfo
        {
            Name = dict["name"].AsString(),
            MaxPlayers = dict["max_players"].AsInt32(),
            UsedPort = dict["used_port"].AsInt32()
        };
    }

    // For easy debugging
    public override string ToString()
    {
        return $"ServerInfo(Name: {Name}, MaxPlayers: {MaxPlayers}, Port: {UsedPort})";
    }
}