using Godot;
using System;
using System.Linq;

public partial class Hud : CanvasLayer
{
	private Camera3D playerCamera;
	private SubViewport subViewport;
	private SubViewportContainer subViewportContainer;

	public Marker3D playerMarker { get; set; }

	public override void _Ready()
	{
		// Main in-game camera
		playerCamera = GetNode<Camera3D>("%ViewportCamera");

		// SubViewport
		subViewportContainer = GetNode<SubViewportContainer>("SubViewportContainer");
		subViewport = GetNode<SubViewport>("SubViewportContainer/SubViewport");

		// Get sub-viewport camera (inside sub-viewport scene)
		Camera3D viewportCamera = subViewport.GetNode<Camera3D>("ViewportCamera");

	}

	public override void _Process(double delta)
	{
		// Update main camera to follow player
		if (playerMarker != null)
		{
			playerCamera.GlobalTransform = playerMarker.GlobalTransform;
		}
	}
}