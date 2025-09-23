using Godot;
using Godot.Collections;

[System.Serializable]
public class ObjectDefinition
{
    public Globals.Classes.ObjectType Type { get; set; } = Globals.Classes.ObjectType.Unknown;
    public long ObjectId { get; set; } = 0;
    public Transform3D Transform { get; set; } = Transform3D.Identity;
    
    // Convert to a Godot Dictionary (storable in Variant)
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            ["type"] = (int)Type,
            ["object_id"] = ObjectId,
            ["transform"] = Transform
        };
    }

    // Reconstruct from Dictionary
    public static ObjectDefinition Deserialize(Dictionary dict)
    {
        return new ObjectDefinition
        {
            Type = (Globals.Classes.ObjectType)(int)dict["type"],
            ObjectId = (long)dict["object_id"],
            Transform = (Transform3D)dict["transform"]
        };
    }
}