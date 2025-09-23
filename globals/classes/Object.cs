using Godot;

public partial class Object : Node3D
{
    public ObjectDefinition Definition { get; set; }
    public virtual void LoadDefinition()
    {
        if (Definition != null)
        {
            GlobalTransform = Definition.Transform;
        }
    }

    public virtual void UpdateDefinition()
    {
        if (Definition != null)
        {
            Definition.Transform = GlobalTransform;
        }
    }
}