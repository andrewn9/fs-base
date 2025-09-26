using Godot;
using System;

public partial class HeadCull : Camera3D
{

	[Export]
    public MeshInstance3D _meshInstance;
    public ShaderMaterial _shaderMaterial;
	public override void _Ready()
	{
		_shaderMaterial = _meshInstance.GetActiveMaterial(0) as ShaderMaterial;
	}


    public override void _Process(double delta)
    {
		// Pass the camera position to the shader uniform
		float pitch = Rotation.X % (Mathf.Pi * 2);
		float FADE_DISTANCE = (float)(-1.527*pitch < 0 ? 0 : -1.527*pitch > 0.6 ? 0.6f : -1.527*pitch);
		_shaderMaterial.SetShaderParameter("FADE_DISTANCE", FADE_DISTANCE);

        _shaderMaterial.SetShaderParameter("CAMERA_POSITION", GlobalPosition);
    }
}
