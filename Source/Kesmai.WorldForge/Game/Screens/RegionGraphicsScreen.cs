using System;
using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.Messaging;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Game.UI.Rendering;
using DigitalRune.Graphics;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Windows;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Kesmai.WorldForge;

public class RegionGraphicsScreen : WorldGraphicsScreen
{
	private static List<Keys> _selectorKeys = new List<Keys>()
	{
		Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6, Keys.F7, Keys.F8
	};

	private static List<Keys> _toolKeys = new List<Keys>()
	{
		Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8
	};
	
	protected RegionToolbar _toolbar;
	protected RegionFilters _filters;
	protected RegionVisibility _visibility;
	
	private StackPanel _componentsHost;
	private ComponentsPanel _componentsPanel;
	private StackPanel _finalizePanel;
	
	private Button _resetButton;

	public override bool DisplayComments => _visibility.ShowComments;

	public RegionGraphicsScreen(IGraphicsService graphicsService, WorldPresentationTarget worldPresentationTarget) : base(graphicsService, worldPresentationTarget)
	{
		var services = ServiceLocator.Current;
		
		_toolbar = services.GetInstance<RegionToolbar>();
		_filters = services.GetInstance<RegionFilters>();
		_visibility = services.GetInstance<RegionVisibility>();
		
		WeakReferenceMessenger.Default.Register<RegionGraphicsScreen, SelectionChanged>(this, (_, message) =>
		{
			var selection = message.Value;

			if (selection.Region != _worldPresentationTarget.Region || _componentsHost is null)
				return;

			_componentsHost.Children.Clear();
			_componentsPanel = null;

			if (_finalizePanel != null)
				_finalizePanel.IsVisible = false;

			// we only care about single-surface selections in this context.
			if (selection.SurfaceArea is not 1)
				return;
			
			// get the segment tile from selection.
			var region = _worldPresentationTarget.Region;
			var selected = selection.FirstOrDefault();

			if (selected.IsEmpty)
				return;
			
			var segmentTile = region.GetTile(selected.X, selected.Y);

			if (segmentTile is null)
				return;
			
			_componentsHost.Children.Add(_componentsPanel = new ComponentsPanel(region, segmentTile, this));

			if (_finalizePanel != null)
				_finalizePanel.IsVisible = true;
		});
	}
	
	protected override IEnumerable<MenuItem> GetContextMenuItems(int mx, int my)
	{
		var services = ServiceLocator.Current;
		var applicationPresenter = services.GetInstance<ApplicationPresenter>();

		var region = _worldPresentationTarget.Region;
		var segment = applicationPresenter.Segment;
		
		var segmentTile = region.GetTile(mx, my);
		
		yield return _contextMenu.Create("Create Location", createLocation);
		yield return _contextMenu.Create("Create Location Spawner", createLocationSpawner);

		if (_selection.Any() && _selection.IsSelected(mx, my, region))
		{
			yield return _contextMenu.CreateSeparator();
			yield return _contextMenu.Create("Create Region Spawner", createRegionSpawner);
			yield return _contextMenu.Create("Create Subregion", createSubregion);
		}
		
		void createLocation(object sender, EventArgs args)
		{
			var location = new SegmentLocation
			{
				Name = $"Location {segment.Locations.Count + 1}", 
				X = segmentTile.X, Y = segmentTile.Y, Region = region.ID,
			};
			segment.Locations.Add(location);

			// present the new location to the user.
			location.Present(applicationPresenter);
		}
		
		void createLocationSpawner(object sender, EventArgs args)
		{
			var spawn = new LocationSpawner()
			{
				Name = $"Location Spawn {segmentTile.X}, {segmentTile.Y} [{region.ID}]",
				X = segmentTile.X, Y = segmentTile.Y, Region = region.ID,
				
				MinimumDelay = TimeSpan.FromMinutes(15.0),
				MaximumDelay = TimeSpan.FromMinutes(30.0),
			};
			segment.Spawns.Location.Add(spawn);

			// present the new spawn to the user.
			spawn.Present(applicationPresenter);
		}
		
		void createRegionSpawner(object sender, EventArgs args)
		{
			var spawn = new RegionSpawner()
			{
				Name = $"Region Spawn [{region.ID}]",
				Region = region.ID,
				
				MinimumDelay = TimeSpan.FromMinutes(15.0),
				MaximumDelay = TimeSpan.FromMinutes(30.0),
			};
			
			spawn.Inclusions.Clear();

			foreach (var rectangle in _selection)
				spawn.Inclusions.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			segment.Spawns.Region.Add(spawn);

			// present the new spawn to the user.
			spawn.Present(applicationPresenter);
		}

		void createSubregion(object sender, EventArgs args)
		{
			var subregion = new SegmentSubregion
			{
				Name = $"Subregion {segment.Subregions.Count + 1}",
				Region = region.ID,
			};

			foreach (var rectangle in _selection)
				subregion.Rectangles.Add(new SegmentBounds(rectangle.Left, rectangle.Top, rectangle.Right - 1, rectangle.Bottom - 1));

			segment.Subregions.Add(subregion);
			
			// present the new subregion to the user.
			subregion.Present(applicationPresenter);
		}
	}

	protected override void OnInitialize()
	{
		base.OnInitialize();
		
		// set up user interface in the editor.
		var grid = new Grid()
		{
		};
		
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Star) });
		grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
		
		grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Star) });
		grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
		
		// components display
		_componentsHost = new StackPanel()
		{
			Width = 500,
		};
		
		grid.AddChild(_componentsHost, 2, 1);
		
		// save / reset panel
		_finalizePanel = new StackPanel()
		{
			Style = "Client-Content",
			
			Orientation = Orientation.Horizontal,
			
			HorizontalAlignment = HorizontalAlignment.Stretch,
			VerticalAlignment = VerticalAlignment.Bottom,
		};
		
		if (_resetButton is null)
		{
			_resetButton = new Button()
			{
				Content = new TextBlock()
				{
					Text = "Reset", FontSize = 10, HorizontalAlignment = HorizontalAlignment.Center,
					Font = "Tahoma", Foreground = Color.White,
				},
				Style = "Client-Button-Red",
				
				HorizontalAlignment = HorizontalAlignment.Stretch,
				
				ToolTip = "[CONTROL + Z]"
			};
		}
		_resetButton.Click += (o, args) => { _componentsPanel.Reset(); };
		
		_finalizePanel.Children.Add(_resetButton);
		
		grid.AddChild(_finalizePanel, 2, 2);
		
		_uiScreen.Children.Add(grid);
	}

	protected override void OnUpdate(TimeSpan deltaTime)
	{
		var region = _worldPresentationTarget.Region;
		
		// retrieve the current active tool and update the cursor.
		var selectedTool = _toolbar.SelectedTool;

		if (selectedTool != null)
			_worldPresentationTarget.Cursor = selectedTool.Cursor;
		
		base.OnUpdate(deltaTime);
		
		if (!_isMouseDirectlyOver)
			return;
		
		var inputManager = PresentationTarget.InputManager;
		
		// process mouse/touch input.
		if (!inputManager.IsMouseOrTouchHandled)
		{
			// give the selected tool the first chance to handle input.
			if (selectedTool != null)
				selectedTool.OnHandleInput(_worldPresentationTarget, inputManager);
		}
		
		if (inputManager.IsKeyboardHandled)
			return;
		
		// process keyboard prior to mouse/touch.
		var multiplier = 3;

		if (inputManager.IsDown(Keys.LeftShift) || inputManager.IsDown(Keys.RightShift))
			multiplier = 7;

		void shiftMap(int dx, int dy)
		{
			CameraLocation += new Vector2F(dx * multiplier, dy * multiplier);
			inputManager.IsKeyboardHandled = true;
		}
		
		if (inputManager.IsPressed(Keys.W, true))
		{
			shiftMap(0, -1);
		}
		else if (inputManager.IsPressed(Keys.S, true))
		{
			shiftMap(0, 1);
		}
		else if (inputManager.IsPressed(Keys.A, true))
		{
			shiftMap(-1, 0);
		}
		else if (inputManager.IsPressed(Keys.D, true))
		{
			shiftMap(1, 0);
		}
		else if (inputManager.IsPressed(Keys.Home, false))
		{
			CenterCameraOn(0, 0);

			if (_selection != null)
				_selection.Select(new Rectangle(0, 0, 1, 1), region);
			
			inputManager.IsKeyboardHandled = true;
		}
		else if (inputManager.IsReleased(Keys.Delete))
		{
			foreach (var area in _selection)
			{
				for (var x = area.Left; x < area.Right; x++)
				for (var y = area.Top; y < area.Bottom; y++)
				{
					var currentFilter = _filters.SelectedFilter;
						
					var tile = region.GetTile(x, y);
						
					if (tile is null)
						continue;
						
					var validComponents = tile.Components.Where(c => currentFilter.IsValid(c)).ToArray();
						
					foreach (var component in validComponents)
						tile.RemoveComponent(component);
				}

			}

			InvalidateRender();
				
			inputManager.IsKeyboardHandled = true;
		}
		else
		{
			if (!inputManager.IsKeyboardHandled)
			{
				foreach (var selectorKey in _selectorKeys)
				{
					if (!inputManager.IsReleased(selectorKey))
						continue;

					var index = _selectorKeys.IndexOf(selectorKey);
					var filters = _filters.Filters;

					if (index >= 0 && index < filters.Count)
						_filters.SelectFilter(filters[index]);

					inputManager.IsKeyboardHandled = true;
				}
			}

			if (!inputManager.IsKeyboardHandled)
			{
				foreach (var toolKey in _toolKeys)
				{
					if (!inputManager.IsReleased(toolKey))
						continue;

					var index = _toolKeys.IndexOf(toolKey);
					var tools = _toolbar.Tools;

					if (index >= 0 && index < tools.Count)
						_toolbar.SelectTool(tools[index]);

					inputManager.IsKeyboardHandled = true;
				}
			}
		}
	}

	protected override void OnRender(RenderContext context)
	{
		base.OnRender(context);
		
		var graphicsService = context.GraphicsService;
		var spriteBatch = graphicsService.GetSpriteBatch();
		
		spriteBatch.Begin(SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
		
		if (_isMouseOver)
		{
			if (_toolbar.SelectedTool != null)
				_toolbar.SelectedTool.OnRender(context);
		}
		
		spriteBatch.End();
	}

	protected override void OnRenderTerrain(SpriteBatch spriteBatch, SegmentTile segmentTile, TerrainRender render, Rectangle bounds)
	{
		var sprite = render.Layer.Sprite;

		if (sprite != null)
		{
			var spriteBounds = bounds;

			if (sprite.Offset != Vector2F.Zero)
				spriteBounds.Offset((int)Math.Floor(sprite.Offset.X * _zoomFactor), (int)Math.Floor(sprite.Offset.Y * _zoomFactor));

			var color = render.Color;
			
			if (_toolbar.SelectedTool != null && _toolbar.SelectedTool.OnRender(segmentTile, render.Layer, out var overrideColor))
				color = overrideColor;
			
			spriteBatch.Draw(sprite.Texture, spriteBounds.Location.ToVector2(), null, color, 0, Vector2.Zero, _zoomFactor / sprite.Resolution, SpriteEffects.None, 0f);
		}
	}

	protected override void OnAfterRender(SpriteBatch spriteBatch)
	{
		base.OnAfterRender(spriteBatch);

		var services = ServiceLocator.Current;
		var applicationPresenter = services.GetInstance<ApplicationPresenter>();

		var region = _worldPresentationTarget.Region;
		var segment = applicationPresenter.Segment;
		var viewRectangle = GetViewRectangle();

		if (_visibility.ShowComments || _visibility.ShowTeleporters || _visibility.ShowSpawns)
			spriteBatch.FillRectangle(GetRenderRectangle(viewRectangle, viewRectangle),
				Color.FromNonPremultiplied(0, 0, 0, 128));

		if (_visibility.ShowTeleporters)
		{
			var localFillColor = Color.FromNonPremultiplied(255, 91, 0, 50);
			var localBorderColor = Color.FromNonPremultiplied(255, 91, 0, 255);
			var globalFillColor = Color.FromNonPremultiplied(0, 255, 91, 50);
			var globalBorderColor = Color.FromNonPremultiplied(0, 255, 91, 255);

			var global = new List<(int X, int Y)>();
			var local = new List<(int X, int Y)>();

			foreach (var searchRegion in segment.Regions)
			{
				var regionTeleporters = searchRegion.GetTiles()
					.Where(tile => tile.Components.OfType<TeleportComponent>().Any())
					.ToDictionary((tile => tile), tile => tile.Components.OfType<TeleportComponent>().FirstOrDefault());

				foreach (var (segmentTile, teleporter) in regionTeleporters)
				{
					if (searchRegion != region)
					{
						// not in this region, skip unless it's a destination here.
						if (teleporter.DestinationRegion != region.ID)
							continue;

						global.Add(new(teleporter.DestinationX, teleporter.DestinationY));
					}
					else
					{
						// in this region, skip unless it's a source here.
						if (teleporter.DestinationRegion != region.ID)
						{
							global.Add((teleporter.DestinationX, teleporter.DestinationY));
						}
						else
						{
							local.Add((segmentTile.X, segmentTile.Y));
							local.Add((teleporter.DestinationX, teleporter.DestinationY));
						}
					}
				}
			}

			void renderTeleporters(List<(int X, int Y)> teleporters, Color fillColor, Color borderColor)
			{
				foreach (var teleporter in teleporters.Distinct()
					         .Where((entry, _) => viewRectangle.Contains(entry.X, entry.Y)))
				{
					var bounds = GetRenderRectangle(viewRectangle, teleporter.X, teleporter.Y);

					spriteBatch.FillRectangle(bounds, fillColor);
					spriteBatch.DrawRectangle(bounds, borderColor);

					/*_font.Style = MSDFStyle.BoldOutline;
					_font.Stroke = Color.Black;
					_font.DrawString(spriteBatch, RenderTransform.Identity, $"[{teleporter.X},{teleporter.Y}]",
						new Vector2(bounds.Left + 2, bounds.Top + 2), borderColor);*/
				}
			}

			renderTeleporters(global, globalFillColor, globalBorderColor);
			renderTeleporters(local, localFillColor, localBorderColor);
		}

		if (_visibility.ShowSpawns)
		{
			var exclusionBorderColor = Color.FromNonPremultiplied(255, 0, 0, 255);
			var exclusionFillColor = Color.FromNonPremultiplied(100, 50, 50, 150);
			var locationFillColor = Color.FromNonPremultiplied(0, 255, 255, 50);
			var locationBorderColor = Color.FromNonPremultiplied(0, 255, 255, 255);
			var regionFillColor = Color.FromNonPremultiplied(255, 150, 255, 10);
			var regionBorderColor = Color.FromNonPremultiplied(255, 0, 255, 255);

			//doing exclusions first for visibility. They are very opaque so obscure anything below
			foreach (var regionSpawner in segment.Spawns.Region)
			{
				if (regionSpawner.Region != region.ID)
					continue;
				
				foreach (var exclusion in regionSpawner.Exclusions)
				{
					if (!exclusion.IsValid || !viewRectangle.Intersects(exclusion.ToRectangle()))
						continue;

					var bounds = GetRenderRectangle(viewRectangle, exclusion.ToRectangle());

					spriteBatch.FillRectangle(bounds, exclusionFillColor);
					spriteBatch.DrawRectangle(bounds, exclusionBorderColor);
				}
			}

			foreach (var locationSpawner in segment.Spawns.Location)
			{
				if (locationSpawner.Region != region.ID)
					continue;
				
				var mx = locationSpawner.X;
				var my = locationSpawner.Y;
				
				if (viewRectangle.Contains(mx, my))
				{
					var bounds = GetRenderRectangle(viewRectangle, mx, my);

					spriteBatch.FillRectangle(bounds, locationFillColor);
					spriteBatch.DrawRectangle(bounds, locationBorderColor);
				}
			}

			foreach (var regionSpawner in segment.Spawns.Region)
			{
				if (regionSpawner.Region != region.ID)
					continue;
				
				foreach (var inclusion in regionSpawner.Inclusions)
				{
					if (!viewRectangle.Intersects(inclusion.ToRectangle()))
						continue;
					
					var bounds = GetRenderRectangle(viewRectangle, inclusion.ToRectangle());

					spriteBatch.FillRectangle(bounds, regionFillColor);
					spriteBatch.DrawRectangle(bounds, regionBorderColor);
				}
			}
		}
	}
}