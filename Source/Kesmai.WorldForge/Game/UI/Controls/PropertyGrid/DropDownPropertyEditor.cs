using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows;

public class DropDownPropertyEditor : PropertyEditor
{
	private PropertyFrame _parent;
	private DropDownButton _internal;

	private IItemsSource _itemsSource;
		
	public DropDownPropertyEditor(ItemsSourceAttribute attribute)
	{
		var sourceType = attribute.Type;
		var sourceItems = Activator.CreateInstance(sourceType) as IItemsSource;

		_itemsSource = sourceItems;
			
		HorizontalAlignment = HorizontalAlignment.Stretch;
	}

	protected override void OnLoad()
	{
		base.OnLoad();
			
		_internal = new DropDownButton()
		{
			Width = 150,
		};
			
		_internal.CreateControlForItem += (o) =>
		{
			var caption = o.ToString();

			if (o is Item item)
				caption = item.DisplayName;
				
			return new TextBlock()
			{
				Text = caption,
				Font = "Tahoma", FontSize = 10,
					
				Foreground = Color.Yellow,
				Shadow = Color.Black,
			};
		};

		var sources = _itemsSource.GetValues();
			
		foreach (var item in sources)
			_internal.Items.Add(item);
			
		var parent = _parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

		if (parent != null)
		{
			var propertyInfo = parent.PropertyInfo;
			var source = parent.Source;
			var value = propertyInfo.GetValue(source);

			var indexOf = sources.IndexOf(sources.First(i => i.Value == value), 0);

			if (indexOf >= 0)
				_internal.SelectedIndex = indexOf;
		}

		Children.Add(_internal);

		_internal.Properties.Get<int>(DropDownButton.SelectedIndexPropertyId).Changed += (o, args) =>
		{
			_parent.PropertyInfo.SetValue(_parent.Source, sources[args.NewValue].Value, null);
		};
	}
}