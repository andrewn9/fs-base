using Godot;
using Godot.Collections;

[System.Serializable]
public class ClientInfo
{
    public string Name { get; set; } = "Unknown";
    public long Id { get; set; } = 0;

    public ClientInfo()
    {
    }

    public ClientInfo(string name, long id)
    {
        Name = name;
        Id = id;
    }

    public Dictionary Serialize()
    {
        return new Dictionary
        {
            { "name", Name },
            { "id", Id }
        };
    }

    public static ClientInfo Deserialize(Dictionary dict)
    {
        return new ClientInfo
        {
            Name = dict["name"].AsString(),
            Id = dict["id"].AsInt64()
        };
    }

    public override string ToString()
    {
        return $"ClientInfo(Name: {Name}, Id: {Id})";
    }
}