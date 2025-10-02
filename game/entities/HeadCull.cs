using Godot;
using System;
using System.Collections.Generic;

public partial class HeadCull : Camera3D
{

	[Export]
	public Skeleton3D skeleton;
	public MeshInstance3D[] _meshInstances;
    public ShaderMaterial[] _shaderMaterials;
	bool isCulled = false;
	public override void _Ready()
	{
		skeleton = GetParent().GetNode<Skeleton3D>("./Model/Skeleton3D");
		// // for each skeleton child, add a mesh instance to the list
		var meshList = new List<MeshInstance3D>();
		for (int i = 0; i < skeleton.GetChildCount(); i++)
		{
			if (skeleton.GetChild(i) is MeshInstance3D meshInstance && meshInstance != null)
			{
				GD.Print(meshInstance.Name);
				meshList.Add(meshInstance);
			}
		}
		_meshInstances = meshList.ToArray();

		_shaderMaterials = new ShaderMaterial[_meshInstances.Length];
		GD.Print(_meshInstances);
		for (int i = 0; i < _meshInstances.Length; i++)
		{
			_shaderMaterials[i] = _meshInstances[i].GetActiveMaterial(0) as ShaderMaterial;
		}

		GD.Print($"IsMultiplayerAuthority(): {IsMultiplayerAuthority()}");
		if (IsMultiplayerAuthority())
		{
			ToggleDither(true);
		}
		else
		{
			ToggleDither(false);
		}
	}

	public void ToggleDither(bool enabled)
	{
		for (int i = 0; i < _meshInstances.Length; i++)
		{
			_meshInstances[i].SetInstanceShaderParameter("Dither", enabled);
		}
	}

    public override void _Process(double delta)
	{
	}
}
