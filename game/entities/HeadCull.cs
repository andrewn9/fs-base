using Godot;
using System;

public partial class HeadCull : Camera3D
{
    [Export]
    public NodePath MeshInstancePath;

    private MeshInstance3D _meshInstance;
    private ShaderMaterial _shaderMaterial;

    public override void _Ready()
    {
        // Get the MeshInstance3D node from the exported path
        _meshInstance = GetNode<MeshInstance3D>(MeshInstancePath);

        // Assume the mesh uses a ShaderMaterial on material_override
        // _shaderMaterial = _meshInstance.MaterialOverride.NextPass as ShaderMaterial;
        // if (_shaderMaterial == null)
        // {
        //     GD.PrintErr("MaterialOverride is not a ShaderMaterial!");
        // }
    }

    public override void _Process(double delta)
    {
        // if (_shaderMaterial == null)
        //     return;

        // // Get the current camera position from the active viewport camera
        // Camera3D camera = GetViewport().GetCamera3D();
        // if (camera == null)
        //     return;

        // Vector3 cameraPos = camera.GlobalPosition;

        // // Pass the camera position to the shader uniform
        // _shaderMaterial.SetShaderParameter("camera_position", cameraPos);
    }
}
