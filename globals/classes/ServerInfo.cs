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
        Name = name ?? "Server";
        MaxPlayers = maxPlayers >= 0 ? maxPlayers : 0;
        UsedPort = usedPort > 0 ? usedPort : 0;
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

    public static ServerInfo Deserialize(Dictionary dict)
    {
        return new ServerInfo
        (
            dict["name"].AsString(),
            dict["max_players"].AsInt32(),
            dict["used_port"].AsInt32()
        );
    }

    public override string ToString()
    {
        return $"ServerInfo(Name: {Name}, MaxPlayers: {MaxPlayers}, Port: {UsedPort})";
    }
}