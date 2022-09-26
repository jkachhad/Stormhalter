using System;
using System.Collections;
using System.Linq;
using CommonServiceLocator;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Microsoft.Toolkit.Mvvm.Messaging;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows
{
	public class ShiftRegionWindow : Window
	{
		private SegmentRegion _region;

		private TextBox _xTextBox;
		private TextBox _yTextBox;
		
		public ShiftRegionWindow(SegmentRegion region)
		{
			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;

			Title = $"Shifting region {region.Name}";

			_region = region;
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			var content = new Canvas()
			{
				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,
			};
			
			var xCoordinatePanel = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
				Padding = new Vector4F(5, 5, 0, 0),
			};
			var xCoordinateTextBlock = new TextBlock()
			{
				Font = "Tahoma", FontSize = 10,
				Text = "X Offset:",
				
				Foreground = Color.Yellow,
				Shadow = Color.Black,
				
				VerticalAlignment = VerticalAlignment.Center,
			};
			_xTextBox = new TextBox()
			{
				Font = "Tahoma", FontSize = 10,
				Text = "0",
				
				Foreground = Color.Yellow,
				
				MinWidth = 100,
				Margin = new Vector4F(3, 0, 0, 0),
			};
			xCoordinatePanel.Children.Add(xCoordinateTextBlock);
			xCoordinatePanel.Children.Add(_xTextBox);
			
			var yCoordinatePanel = new StackPanel()
			{
				Y = 30,
				Orientation = Orientation.Horizontal,
				Padding = new Vector4F(5, 5, 0, 0),
			};
			var yCoordinateTextBlock = new TextBlock()
			{
				Font = "Tahoma", FontSize = 10,
				Text = "Y Offset:",
				
				Foreground = Color.Yellow,
				Shadow = Color.Black,
				
				VerticalAlignment = VerticalAlignment.Center,
			};
			_yTextBox = new TextBox()
			{
				Font = "Tahoma", FontSize = 10,
				Text = "0",
				
				Foreground = Color.Yellow,
				
				MinWidth = 100,
				Margin = new Vector4F(3, 0, 0, 0),
			};
			yCoordinatePanel.Children.Add(yCoordinateTextBlock);
			yCoordinatePanel.Children.Add(_yTextBox);
			
			var okButton = new Button()
			{
				Margin = new Vector4F(5),
				
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				
				Content = new TextBlock()
				{
					Margin = new Vector4F(3),
					
					Font = "Tahoma", FontSize = 10,
					Text = "OK",
					
					Foreground = Color.LimeGreen,
					Shadow = Color.Black,
				}
			};
			okButton.Click += Shift;
			
			content.Children.Add(xCoordinatePanel);
			content.Children.Add(yCoordinatePanel);
			content.Children.Add(okButton);

			Content = content;
		}

		private void Shift(object sender, EventArgs e)
		{
			if (!int.TryParse(_xTextBox.Text, out var sx) || !int.TryParse(_yTextBox.Text, out var sy))
				return;

			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;

			var oldTiles = _region.GetTiles((tile) => true).ToList();

			foreach (var tile in oldTiles)
				_region.DeleteTile(tile.X, tile.Y);

			foreach (var tile in oldTiles)
			{
				var dx = tile.X + sx;
				var dy = tile.Y + sy;
				
				var duplicate = new SegmentTile(dx, dy);

				foreach (var component in tile.Components)
					duplicate.AddComponent(component);

				_region.SetTile(dx, dy, duplicate);
			}
			
			/* */
			foreach (var region in segment.Regions)
			{
				var linkedComponents = region.GetTiles((tile) => true)
					.SelectMany((tile) => tile.GetComponents<TeleportComponent>()
						.Where((t) => t.DestinationRegion == _region.ID));

				foreach (var linked in linkedComponents)
				{
					linked.DestinationX += sx;
					linked.DestinationY += sy;
				}
			}
		}
	}
}