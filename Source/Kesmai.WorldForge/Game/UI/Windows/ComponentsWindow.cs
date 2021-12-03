using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
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
		private StackPanel _leftPanel;

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
			_leftPanel = new StackPanel()
			{
				Background = Color.DarkRed,
			};
			content.Children.Add(_leftPanel);

			foreach (var component in _tile.Components)
			{
				var frame = new ComponentFrame()
				{
					Component = component,
				};
				frame.Click += (o, args) => { Select(o as ComponentFrame); };

				_leftPanel.Children.Add(frame);
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
			
			Select(_leftPanel.Children.OfType<ComponentFrame>().FirstOrDefault());
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
					var index = _tile.Components.IndexOf(SelectedItem.Component);
					_tile.RemoveComponent(SelectedItem.Component);
					OnLoad(); // update the component editor UI
					if (index > _leftPanel.Children.Count - 1)
						index = _leftPanel.Children.Count - 1;
					if(index!= -1)
						Select(_leftPanel.Children.ElementAt(index) as ComponentFrame);
					_screen.InvalidateRender(); //redraw the worldscreen
				};

				var moveUpButton = new Button()
				{
					Content = new TextBlock()
					{
						Foreground = Color.OrangeRed,
						Shadow = Color.Black,

						Font = "Tahoma14Bold",
						Text = "Move up",

						Margin = new Vector4F(3, 3, 3, 3)
					}
				};
				moveUpButton.Click += (o, args) =>
				{
					var index = _tile.Components.IndexOf(SelectedItem.Component);
					if (index > 0)
					{
						_tile.Components.Move(index, index - 1);
						OnLoad();
						Select(_leftPanel.Children.ElementAt(index-1) as ComponentFrame);
					}
				};

				var moveDownButton = new Button()
				{
					Content = new TextBlock()
					{
						Foreground = Color.OrangeRed,
						Shadow = Color.Black,

						Font = "Tahoma14Bold",
						Text = "Move down",

						Margin = new Vector4F(3, 3, 3, 3)
					}
				};
				moveDownButton.Click += (o, args) =>
				{
					var index = _tile.Components.IndexOf(SelectedItem.Component);
					if (index < _tile.Components.Count-1)
					{
						_tile.Components.Move(index, index + 1);
						OnLoad();
						Select(_leftPanel.Children.ElementAt(index + 1) as ComponentFrame);
					}
				};

				_actionsPanel.Children.Add(deleteButton);
				_actionsPanel.Children.Add(moveUpButton);
				_actionsPanel.Children.Add(moveDownButton);


				if (component is TeleportComponent)
                {
					var configureButton = new Button()
					{
						Content = new TextBlock()
						{
							Foreground = Color.OrangeRed,
							Shadow = Color.Black,

							Font = "Tahoma14Bold",
							Text = "Select Destination",

							Margin = new Vector4F(3, 3, 3, 3)
						}
					};
					configureButton.Click += (o, args) =>
					{
						var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();
						presenter.ConfiguringTeleporter = component as TeleportComponent;
						Close();
					};
					_actionsPanel.Children.Add(configureButton);
				}


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
				if (inputService.IsDown(Keys.LeftControl) || inputService.IsDown(Keys.RightControl))
				{
					if (inputService.IsReleased(Keys.C))
                    {
						Clipboard.SetText(SelectedItem.Component.GetXElement().ToString());
                    }
					if (inputService.IsReleased(Keys.V))
					{
						try { 
							if (Clipboard.GetText() is string clipboard && XDocument.Parse(clipboard) is XDocument element && element.Root.Name.ToString() == "component")
							{
								var assembly = Assembly.GetExecutingAssembly();
								var componentTypename = $"Kesmai.WorldForge.Models.{element.Root.Attribute("type").Value}";
								var componentType = assembly.GetType(componentTypename, true);

								if (Activator.CreateInstance(componentType, element.Root) is TerrainComponent component)
								{
									_tile.AddComponent(component);
									OnLoad();
									Select(_leftPanel.Children.OfType<ComponentFrame>().LastOrDefault());
								}
							}
						} catch { } // ignore errors in this section. They are probably malformatted XML or other clipboard-is-not-relevant issues
					}
				}
			}

			if (IsActive)
				inputService.IsKeyboardHandled = true;
		}
	}
}