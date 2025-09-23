using Godot;
using Godot.Collections;

using System;
[Serializable]
public class ClientInfo
{
    private string _name = "Unknown";
    private long _id = 0;

    public string Name
    {
        get => _name;
        set => _name = value ?? "Unknown";
    }

    public long Id
    {
        get => _id;
        set => _id = value >= 0 ? value : 0;
    }

    public ClientInfo() { }

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