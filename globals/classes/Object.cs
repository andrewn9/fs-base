using Godot;

public partial class Object : Node3D
{
    private ObjectDefinition _definition;

    public ObjectDefinition Definition
    {
        get => _definition;
        set => _definition = value ?? new ObjectDefinition();
    }

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