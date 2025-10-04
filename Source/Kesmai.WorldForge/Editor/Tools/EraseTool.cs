using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class EraseTool : ComponentTool
{
	private bool _isShiftDown;
	private bool _isAltDown;

	protected override Color SelectionColor => Color.Red;

	public EraseTool() : base("Erase", @"Editor-Icon-Erase")
	{
	}

	public override bool OnRender(SegmentTile tile, TerrainLayer layer, out Color overrideColor)
	{
		if (base.OnRender(tile, layer, out overrideColor))
			return true;
		
		if (_tileUnderMouse != tile)
			return false;
		
		if (_componentUnderMouse != null && _componentUnderMouse.GetTerrain().Any(t => t.Terrain.Contains(layer)))
		{
			overrideColor = Color.Red;
			return true;
		}

		return false;
	}

	public override void OnHandleInput(WorldPresentationTarget target, IInputService inputService)
	{
		base.OnHandleInput(target, inputService);
			
		if (inputService.IsMouseOrTouchHandled)
			return;
			
		if (!inputService.IsKeyboardHandled)
		{
			_isAltDown = inputService.IsDown(Keys.LeftAlt) || inputService.IsDown(Keys.RightAlt);
			_isShiftDown = inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift);
		}
	}

	protected override void OnClick()
	{
		base.OnClick();
			
		if (!_isAltDown && !_isShiftDown)
		{
			_tileUnderMouse.Components.Remove(_componentUnderMouse);
		}
		else if (_isAltDown)
		{
			_tileUnderMouse.Components.Clear();
		}
	}
		
	private Cursor _removeNormal;

	public override void OnActivate()
	{
		base.OnActivate();
		var removeNormal = Application.GetResourceStream(
			new Uri(@"/Kesmai.WorldForge;component/Resources/Remove-Normal.cur", UriKind.Relative));

		if (removeNormal != null)
			_removeNormal = new Cursor(removeNormal.Stream);
		else
			_removeNormal = Cursors.Arrow;
		
		_cursor = _removeNormal;
	}
		
	public override void OnDeactivate()
	{
		_cursor = Cursors.Arrow;
		base.OnDeactivate();
	}
}