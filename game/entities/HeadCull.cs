using Godot;
using System;

public partial class HeadCull : Camera3D
{

	[Export]
	public MeshInstance3D[] _meshInstances;
    public ShaderMaterial[] _shaderMaterials;
	bool isCulled = false;
	public override void _Ready()
	{
		_shaderMaterials = new ShaderMaterial[_meshInstances.Length];
		for (int i = 0; i < _meshInstances.Length; i++)
		{
			_shaderMaterials[i] = _meshInstances[i].GetActiveMaterial(0) as ShaderMaterial;
		}
		GD.Print($"IsMultiplayerAuthority(): {IsMultiplayerAuthority()}");
		if (IsMultiplayerAuthority())
		{
			for (int i = 0; i < _meshInstances.Length; i++)
			{
				_meshInstances[i].SetInstanceShaderParameter("DISABLED", false);
			}
		}
		else
		{
			for (int i = 0; i < _meshInstances.Length; i++)
			{
				_meshInstances[i].SetInstanceShaderParameter("DISABLED", true);
			}
		}
	}

    public override void _Process(double delta)
	{
	}
}
