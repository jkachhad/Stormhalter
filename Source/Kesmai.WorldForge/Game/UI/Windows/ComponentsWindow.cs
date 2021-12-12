using System.Collections;
using System.Linq;
using System.Windows;
using CommonServiceLocator;
using DigitalRune;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using HorizontalAlignment = DigitalRune.Game.UI.HorizontalAlignment;
using VerticalAlignment = DigitalRune.Game.UI.VerticalAlignment;
using Window = DigitalRune.Game.UI.Controls.Window;

namespace Kesmai.WorldForge.Windows
{
	public class ComponentsWindow : Window
	{
		private SegmentTile _tile;
		private WorldGraphicsScreen _screen;
		private PropertyGrid _propertyGrid;
		private StackPanel _actionsPanel;
		
		public static readonly int SelectedItemPropertyId = CreateProperty(
			typeof(ComponentsWindow), "SelectedItem", GamePropertyCategories.Default, null, default(ComponentFrame),
			UIPropertyOptions.AffectsRender);

		public ComponentFrame SelectedItem
		{
			get => GetValue<ComponentFrame>(SelectedItemPropertyId);
			set => SetValue(SelectedItemPropertyId, value);
		}
		
		public ComponentsWindow(SegmentRegion region, SegmentTile tile, WorldGraphicsScreen screen)
		{
			_tile = tile;
			_screen = screen;
			
			Title = $"Editing components for {tile.X}, {tile.Y} [{region.ID}]";
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			
			var content = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
			};

			/* Left Panel */
			var leftPanel = new StackPanel()
			{
				Background = Color.DarkRed,
			};
			content.Children.Add(leftPanel);

			foreach (var component in _tile.Components)
			{
				var frame = new ComponentFrame()
				{
					Component = component,
				};
				frame.Click += (o, args) => { Select(o as ComponentFrame); };
				
				leftPanel.Children.Add(frame);
			}
			
			/* Right Panel */
			var rightPanel = new StackPanel()
			{
			};
			content.Children.Add(rightPanel);
			
			_propertyGrid = new PropertyGrid()
			{
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			rightPanel.Children.Add(_propertyGrid);
			_propertyGrid.PropertyChanged += (o, args) =>
			{
				_tile.UpdateTerrain(); //update the tile and redraw the world screen
				_screen.InvalidateRender();
			};
			
			_actionsPanel = new StackPanel()
			{
				Style = "DarkCanvas",
				
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Stretch,
			};
			rightPanel.Children.Add(_actionsPanel);
			
			Content = content;
			
			Select(leftPanel.Children.OfType<ComponentFrame>().FirstOrDefault());
		}

		public void Select(ComponentFrame frame)
		{
			SelectedItem = frame;

			if (frame != null)
			{
				var component = frame.Component;
				
				_propertyGrid.Item = component;
				
				_actionsPanel.Children.Clear();
				
				foreach (var button in SelectedItem.Component.GetInspectorActions())
					_actionsPanel.Children.Add(button);

				var deleteButton = new Button()
				{
					Content = new TextBlock()
					{
						Foreground = Color.OrangeRed,
						Shadow = Color.Black,

						Font = "Tahoma14Bold",
						Text = "Delete",

						Margin = new Vector4F(3, 3, 3, 3)
					}
				};

				deleteButton.Click += (o, args) =>
				{
					_tile.RemoveComponent(SelectedItem.Component);
					OnLoad(); // update the component editor UI
					_screen.InvalidateRender(); //redraw the worldscreen
				};

				_actionsPanel.Children.Add(deleteButton);
			}
		}

		protected override void OnRender(UIRenderContext context)
		{
			base.OnRender(context);
			
			var selected = SelectedItem;

			if (selected != null)
			{
				var selectedBounds = selected.ActualBounds.ToRectangle(true);

				var screen = context.Screen ?? Screen;
				var renderer = screen.Renderer;
				var spriteBatch = renderer.SpriteBatch;
				
				spriteBatch.FillRectangle(selectedBounds, Color.FromNonPremultiplied(0, 100, 255, 25));
			}
		}

		protected override void OnHandleInput(InputContext context)
		{
			if (!IsVisible)
				return;
			
			base.OnHandleInput(context);

			var inputService = InputService;

			if (inputService == null)
				return;

			if (!inputService.IsKeyboardHandled)
			{
				if (inputService.IsReleased(Keys.Escape))
				{
					Close();
				}
			}

			if (IsActive)
				inputService.IsKeyboardHandled = true;
		}
	}
}