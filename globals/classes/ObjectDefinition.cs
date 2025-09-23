using Godot;
using Godot.Collections;

[System.Serializable]
public class ObjectDefinition
{
    public Globals.Classes.ObjectType objectType { get; set; } = Globals.Classes.ObjectType.Unknown;
    public long objectId { get; set; } = 0;
    public Transform3D Transform { get; set; } = Transform3D.Identity;
    
    // Convert to a Godot Dictionary (storable in Variant)
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            ["type"] = (int) objectType,
            ["object_id"] = objectId,
            ["transform"] = Transform
        };
    }

    // Reconstruct from Dictionary
    public static ObjectDefinition Deserialize(Dictionary dict)
    {
        return new ObjectDefinition
        {
            objectType = (Globals.Classes.ObjectType)(int)dict["type"],
            objectId = (long)dict["object_id"],
            Transform = (Transform3D)dict["transform"]
        };
    }
}