using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows;

public class ColorPropertyEditor : PropertyEditor
{
	private PropertyFrame _parent;

	private Slider[] _sliders;
	private TextBlock[] _labels;

	public ColorPropertyEditor()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
	}
		
	protected override void OnLoad()
	{
		base.OnLoad();

		var stackPanel = new StackPanel()
		{
			HorizontalAlignment = HorizontalAlignment.Stretch,
		};
			
		var colors = new Color[]
		{
			Color.Red,
			Color.LimeGreen,
			Color.Aqua,
			Color.White
		};
			
		_sliders = new Slider[4];
		_labels = new TextBlock[4];
			
		for (int i = 0; i < 4; i++)
		{
			var sliderPanel = new StackPanel()
			{
				Orientation = Orientation.Horizontal,
			};
				
			sliderPanel.Children.Add(_sliders[i] = new Slider()
			{
				Width = 150,
				Minimum = 0, Maximum = 255,
			});
			sliderPanel.Children.Add(_labels[i] = new TextBlock()
			{
				Font = "Tahoma", FontSize = 10,
					
				Foreground = colors[i], Stroke = Color.Black,
				FontStyle = MSDFStyle.Outline,
					
				Margin = new Vector4F(3, 0, 0, 0)
			});
				
			stackPanel.Children.Add(sliderPanel);
		}
			
		Children.Add(stackPanel);
			
		var parent = _parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

		if (parent != null)
		{
			var propertyInfo = parent.PropertyInfo;
			var source = parent.Source;

			var color = (Color)propertyInfo.GetValue(source);

			_sliders[0].Value = color.R;
			_sliders[1].Value = color.G;
			_sliders[2].Value = color.B;
			_sliders[3].Value = color.A;

			for (int i = 0; i < 4; i++)
				_sliders[i].Properties.Get<float>(RangeBase.ValuePropertyId).Changed += (o, args) => Update();

			Update();
		}
		else
		{
			throw new Exception("Unable to find parent frame for PropertyEditor");
		}
	}

	private void Update()
	{
		for (int i = 0; i < 4; i++)
			_labels[i].Text = $"{(int)_sliders[i].Value}";

		var color = new Color(
			(int)_sliders[0].Value, 
			(int)_sliders[1].Value, 
			(int)_sliders[2].Value,
			(int)_sliders[3].Value);
			
		if (_parent != null)
		{
			var propertyInfo = _parent.PropertyInfo;
			var source = _parent.Source;
				
			propertyInfo.SetValue(source, color, null);
			
			NotifyPropertyChanged(propertyInfo);
		}
	}
}