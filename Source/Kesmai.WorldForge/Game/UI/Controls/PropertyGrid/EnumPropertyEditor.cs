using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows;

public class EnumPropertyEditor : PropertyEditor
{
	private PropertyFrame _parent;
	private DropDownButton _dropDownButton;

	public EnumPropertyEditor()
	{
		HorizontalAlignment = HorizontalAlignment.Stretch;
	}

	protected override void OnLoad()
	{
		base.OnLoad();
			
		_dropDownButton = new DropDownButton()
		{
			Width = 150,
		};
			
		_dropDownButton.CreateControlForItem += (o) =>
		{
			return new TextBlock()
			{
				Text = o.ToString(),
				Font = "Tahoma", FontSize = 10,
			};
		};

		_dropDownButton.Properties.Get<int>(DropDownButton.SelectedIndexPropertyId).Changed += (o, args) =>
		{
			if (_parent != null)
			{
				var propertyInfo = _parent.PropertyInfo;
				var source = _parent.Source;
					
				var values = Enum.GetValues(propertyInfo.PropertyType);
				var value = values.GetValue(args.NewValue);
					
				propertyInfo.SetValue(source, value, null);
				
				NotifyPropertyChanged(propertyInfo);
			}
		};
			
		var parent = _parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

		if (parent != null)
		{
			var propertyInfo = parent.PropertyInfo;
			var source = parent.Source;

			var values = Enum.GetValues(propertyInfo.PropertyType);

			foreach (var item in values)
				_dropDownButton.Items.Add(item);
				
			var value = propertyInfo.GetValue(source);
				
			_dropDownButton.SelectedIndex = Array.IndexOf(values, value);
		}

		Children.Add(_dropDownButton);
	}
}