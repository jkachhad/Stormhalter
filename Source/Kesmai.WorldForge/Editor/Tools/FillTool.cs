using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Game.Input;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

/// <summary>
/// Places the selected component on the connected area of matching tiles under the cursor.
/// </summary>
/// <remarks>
/// Tiles match when their components that pass the current filter are the same, so with the floor filter a fill
/// spreads across tiles sharing a floor whatever else is on them. Clicking inside the selection keeps the fill
/// within it, which is also how to fill empty space. Shift appends and Alt replaces everything, as with Draw.
/// </remarks>
public class FillTool : Tool
{
	/// <summary>
	/// The most tiles one fill may change; a larger area most likely leaked out of the intended one.
	/// </summary>
	private const int MaxFillTiles = 10000;

	private static readonly string[] _floorTypes =
	{
		"FloorComponent",
		"WaterComponent",
		"IceComponent",
		"SkyComponent"
	};

	private bool _isShiftDown;
	private bool _isAltDown;

	public FillTool() : base("Fill", "Editor-Icon-Fill")
	{
	}

	public override void OnActivate()
	{
		base.OnActivate();
		_cursor = Cursors.Arrow;
	}

	public override void OnHandleInput(WorldPresentationTarget target, IInputService inputService)
	{
		base.OnHandleInput(target, inputService);

		if (inputService.IsMouseOrTouchHandled)
			return;

		var services = ServiceLocator.Current;
		var presenter = services.GetInstance<ApplicationPresenter>();
		var regionToolbar = services.GetInstance<RegionToolbar>();
		var regionFilters = services.GetInstance<RegionFilters>();
		var componentPalette = services.GetInstance<ComponentPalette>();

		var worldScreen = target.WorldScreen;
		var region = target.Region;
		var selection = presenter.Selection;

		if (!inputService.IsKeyboardHandled)
		{
			_isShiftDown = inputService.IsDown(Keys.LeftShift) || inputService.IsDown(Keys.RightShift);
			_isAltDown = inputService.IsDown(Keys.LeftAlt) || inputService.IsDown(Keys.RightAlt);

			if (inputService.IsReleased(Keys.Escape))
			{
				regionToolbar.SelectTool(null);
				inputService.IsKeyboardHandled = true;
			}
		}

		if (region is null || !inputService.IsReleased(MouseButtons.Left))
			return;

		var provider = componentPalette.SelectedProvider;

		if (provider is null)
			return;

		inputService.IsMouseOrTouchHandled = true;

		var (mx, my) = worldScreen.ToWorldCoordinates((int)_position.X, (int)_position.Y);

		// clicking inside the selection keeps the fill within it.
		var bounds = selection.IsSelected(mx, my, region) ? selection : null;
		var area = FindArea(region, mx, my, regionFilters.SelectedFilter, bounds);

		if (area is null)
		{
			System.Windows.MessageBox.Show(
				$"This fill would change more than {MaxFillTiles:N0} tiles. Select an area and click inside it to limit the fill.",
				"Fill", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
			return;
		}

		var edit = presenter.History.Begin("Fill");

		foreach (var (x, y) in area)
		{
			edit.Capture(region, x, y);

			var tile = region.GetTile(x, y);

			if (tile is null)
				region.SetTile(x, y, tile = new SegmentTile(x, y));

			Place(tile, provider);

			tile.UpdateTerrain();
		}

		edit.Commit();

		worldScreen.InvalidateRender();
	}

	/// <summary>
	/// Finds the tiles connected to the start (sideways, not diagonally) that match it.
	/// </summary>
	/// <returns>The tiles, or null if there are more than <see cref="MaxFillTiles"/>.</returns>
	private static List<(int X, int Y)> FindArea(SegmentRegion region, int startX, int startY,
		TerrainSelector filter, Selection bounds)
	{
		var signature = GetSignature(region.GetTile(startX, startY), filter);

		var area = new List<(int X, int Y)>();
		var visited = new HashSet<(int X, int Y)> { (startX, startY) };
		var queue = new Queue<(int X, int Y)>();

		queue.Enqueue((startX, startY));

		while (queue.Count > 0)
		{
			var (x, y) = queue.Dequeue();

			area.Add((x, y));

			if (area.Count > MaxFillTiles)
				return null;

			foreach (var (nx, ny) in new[] { (x + 1, y), (x - 1, y), (x, y + 1), (x, y - 1) })
			{
				if (!visited.Add((nx, ny)))
					continue;

				if (bounds != null && !bounds.IsSelected(nx, ny, region))
					continue;

				if (GetSignature(region.GetTile(nx, ny), filter) != signature)
					continue;

				queue.Enqueue((nx, ny));
			}
		}

		return area;
	}

	/// <summary>
	/// Describes the tile's components that pass the filter, so matching tiles have equal signatures.
	/// </summary>
	/// <returns>Null for a location with no tile, which only matches other empty locations.</returns>
	private static string GetSignature(SegmentTile tile, TerrainSelector filter)
	{
		if (tile is null)
			return null;

		var builder = new StringBuilder();

		foreach (var provider in tile.Providers)
		{
			if (filter.IsValid(provider))
				builder.Append(provider.GetReferencingElement().ToString(SaveOptions.DisableFormatting));
		}

		return builder.ToString();
	}

	/// <summary>
	/// Places the component on the tile with the Draw tool's rules: by default it replaces components of the same
	/// kind (any floor-type component for floors), Shift appends, and Alt replaces everything.
	/// </summary>
	private void Place(SegmentTile tile, IComponentProvider provider)
	{
		if (!_isAltDown && !_isShiftDown)
		{
			var componentType = provider.GetType();

			IEnumerable<IComponentProvider> similar;

			if (_floorTypes.Contains(componentType.Name))
				similar = tile.GetComponents<TerrainComponent>(c => _floorTypes.Contains(c.GetType().Name));
			else
				similar = tile.GetComponents<IComponentProvider>(c => c.GetType().IsAssignableFrom(componentType));

			foreach (var existing in similar)
				existing.RemoveComponent(tile.Providers);
		}
		else if (_isAltDown)
		{
			tile.Providers.Clear();
		}

		provider.AddComponent(tile.Providers);
	}

	public override void OnRender(RenderContext context)
	{
		base.OnRender(context);

		var graphicsService = context.GraphicsService;
		var spriteBatch = graphicsService.GetSpriteBatch();

		if (context.PresentationTarget is not WorldPresentationTarget presentationTarget)
			return;

		var worldScreen = presentationTarget.WorldScreen;
		var uiScreen = worldScreen.UI;
		var renderer = uiScreen.Renderer;
		var spriteFont = renderer.GetFontRenderer("Tahoma", 10);

		var text = String.Empty;

		if (_isAltDown)
			text = "Replace";
		else if (_isShiftDown)
			text = "Append";

		if (String.IsNullOrEmpty(text))
			return;

		var position = (Vector2)_position + new Vector2(10.0f, -10.0f);

		spriteFont.DrawString(spriteBatch, RenderTransform.Identity, text, position + new Vector2(1f, 1f), Color.Black);
		spriteFont.DrawString(spriteBatch, RenderTransform.Identity, text, position, Color.Yellow);
	}
}
