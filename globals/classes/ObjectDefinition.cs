using Godot;
using Godot.Collections;

[System.Serializable]
public class ObjectDefinition
{
    private Globals.Classes.ObjectType _objectType = Globals.Classes.ObjectType.Unknown;
    private long _objectId = 0;
    private Transform3D _transform = Transform3D.Identity;

    public Globals.Classes.ObjectType ObjectType
    {
        get => _objectType;
        set => _objectType = value;
    }

    public long ObjectId
    {
        get => _objectId;
        set => _objectId = value;
    }

    public Transform3D Transform
    {
        get => _transform;
        set => _transform = value;
    }
    
    // Convert to a Godot Dictionary (storable in Variant)
    public Dictionary Serialize()
    {
        return new Dictionary
        {
            ["type"] = (int) this.ObjectType,
            ["object_id"] = this.ObjectId,
            ["transform"] = this.Transform
        };
    }

    // Reconstruct from Dictionary
    public static ObjectDefinition Deserialize(Dictionary dict)
    {
        var objDef = new ObjectDefinition();
    objDef.ObjectType = dict.ContainsKey("type") ? (Globals.Classes.ObjectType)(int)dict["type"] : Globals.Classes.ObjectType.Unknown;
    objDef.ObjectId = dict.ContainsKey("object_id") ? (long)dict["object_id"] : 0;
    objDef.Transform = dict.ContainsKey("transform") ? (Transform3D)dict["transform"] : Transform3D.Identity;
        return objDef;
    }
}