using System;
using System.Windows;
using System.Windows.Input;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge;

public class HammerTool : ComponentTool
{
	protected override Color SelectionColor => Color.Orange;

	public HammerTool() : base("Indestructible", @"Editor-Icon-Hammer")
	{
	}

	protected override bool IsValid(TerrainComponent component)
	{
		return component is WallComponent;
	}

	protected override void OnClick()
	{
		base.OnClick();
			
		if (_componentUnderMouse is WallComponent wall)
			wall.IsIndestructible = !wall.IsIndestructible;
	}
}