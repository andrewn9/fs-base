using Godot;
using Godot.Collections;

[System.Serializable]
public class CharacterDefinition
{
    public bool exists = false;
    public Vector3 position = Vector3.Zero;
    public Vector2 moveDirection = Vector2.Zero;


    public CharacterDefinition()
    {
    }

    public CharacterDefinition(bool exists, Vector3 position, Vector2 moveDirection)
    {
        this.exists = exists;
        this.position = position;
        this.moveDirection = moveDirection;
    }

    public Dictionary Serialize()
    {
        return new Dictionary
        {
            { "exists", exists },
            { "position", position },
            { "moveDirection", moveDirection}
        };
    }

    public static CharacterDefinition Deserialize(Dictionary dict)
    {
        return new CharacterDefinition
        {
            exists = dict["exists"].AsBool(),
            position = dict["position"].AsVector3(),
            moveDirection = dict["moveDirection"].AsVector2(),
        };
    }

    public override string ToString()
    {
        return "";
        // return $"ClientInfo(Name: {Name}, Id: {Id})";
    }
}