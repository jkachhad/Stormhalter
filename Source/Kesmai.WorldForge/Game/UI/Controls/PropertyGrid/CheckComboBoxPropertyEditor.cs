using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DigitalRune.Collections;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows
{
	public class CheckComboBoxPropertyEditor : PropertyEditor
	{
		private PropertyFrame _parent;
		private CheckComboBoxButton _comboBoxButton;

		private IItemsSource _itemsSource;
		
		public CheckComboBoxPropertyEditor(ItemsSourceAttribute attribute)
		{
			var sourceType = attribute.Type;
			var sourceItems = Activator.CreateInstance(sourceType) as IItemsSource;

			_itemsSource = sourceItems;
			
			HorizontalAlignment = HorizontalAlignment.Stretch;
		}

		protected override void OnLoad()
		{
			base.OnLoad();
			
			_comboBoxButton = new CheckComboBoxButton()
			{
				Width = 150,
			};
			
			_comboBoxButton.CreateControlForItem += (o) =>
			{
				var caption = o.ToString();

				if (o is Item item)
					caption = item.DisplayName;
				
				return new TextBlock()
				{
					Text = caption,
					Font = "Tahoma14Bold",
					
					Foreground = Color.Yellow,
					Shadow = Color.Black,
				};
			};

			var sources = _itemsSource.GetValues();
			
			foreach (var item in sources)
				_comboBoxButton.Items.Add(item);
			
			var parent = _parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

			if (parent != null)
			{
				var propertyInfo = parent.PropertyInfo;
				var source = parent.Source;
				var propValue = propertyInfo.GetValue(source);

				if (propValue is IList values)
				{
					foreach (var value in values)
					{
						var item = sources.FirstOrDefault(i => i.Value == value);

						if (item != null)
							_comboBoxButton.SelectedItems.Add(item);
					}
				}
				if (propValue is Direction dir)
                {
					var item = sources.FirstOrDefault(i => (Direction)i.Value == dir);
					if (item != null)
						_comboBoxButton.SelectedItems.Add(item);
                }
			}

			Children.Add(_comboBoxButton);
			
			_comboBoxButton.SelectedItems.CollectionChanged += Update;
		}

		private void Update(object sender, CollectionChangedEventArgs<object> e)
		{
			if (_parent != null)
			{
				var propertyInfo = _parent.PropertyInfo;
				var source = _parent.Source;
				var propValue = propertyInfo.GetValue(source);

				if (propValue is IList values)
				{
					foreach (var newItem in e.NewItems)
					{
						if (newItem is Item item)
							values.Add(item.Value);
					}
					
					foreach (var oldItem in e.OldItems)
					{
						if (oldItem is Item item)
							values.Remove(item.Value);
					}
					
					propertyInfo.SetValue(source, values, null);
				}

				if (propValue is Direction dir)
                {
					foreach (var newItem in e.NewItems)
					{
						if (newItem is Item item)
							propertyInfo.SetValue(source, (Direction)item.Value, null);
					}
				}
			}
		}
	}
}