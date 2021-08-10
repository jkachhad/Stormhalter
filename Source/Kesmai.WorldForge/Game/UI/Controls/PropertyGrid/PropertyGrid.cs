using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using DigitalRune.Collections;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows
{
	public class PropertyGrid : Canvas
	{
		private ScrollViewer _scrollViewer;
		private StackPanel _internalPanel;
		
		public static readonly int ItemPropertyId = CreateProperty(
			typeof(PropertyGrid), "Item", GamePropertyCategories.Default, null, default(object),
			UIPropertyOptions.AffectsRender);

		public object Item
		{
			get => GetValue<object>(ItemPropertyId);
			set => SetValue(ItemPropertyId, value);
		}
		
		public NotifyingCollection<PropertyInfo> Items { get; set; }
		
		public PropertyGrid()
		{
			Items = new NotifyingCollection<PropertyInfo>();
			Items.CollectionChanged += OnCollectionChanged;

			HorizontalAlignment = HorizontalAlignment.Stretch;
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			_internalPanel = new StackPanel()
			{
				Background = Color.DarkRed,
			};
			_scrollViewer = new ScrollViewer()
			{
				Content = _internalPanel,

				HorizontalAlignment = HorizontalAlignment.Stretch,
				VerticalAlignment = VerticalAlignment.Stretch,

				VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
				HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
				
				Padding = Vector4F.Zero,
				Margin = Vector4F.Zero,
			};
			Children.Add(_scrollViewer);

			Properties.Get<object>(ItemPropertyId).Changed += (o, args) =>
			{
				Update(args.NewValue);
			};
		}

		private void Update(object item)
		{
			Items.Clear();

			var properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var property in properties)
			{
				var browsable = property.GetCustomAttribute<BrowsableAttribute>();

				if (browsable != null && browsable.Browsable)
					Items.Add(property);
			}
		}

		private void OnCollectionChanged(object sender, CollectionChangedEventArgs<PropertyInfo> args)
		{
			var updateNew = (args.Action == CollectionChangedAction.Add);
			var updateOld = (args.Action == CollectionChangedAction.Remove || args.Action == CollectionChangedAction.Clear);

			if (args.Action == CollectionChangedAction.Replace)
				updateNew = updateOld = true;

			if (updateNew)
			{
				args.NewItems.ForEach(propertyInfo =>
				{
					_internalPanel.Children.Add(new PropertyFrame()
					{
						PropertyInfo = propertyInfo,
						Source = Item,
					});
				});
			}

			if (updateOld)
			{
				args.OldItems.ForEach(propertyInfo =>
				{
					var frame = _internalPanel.Children.OfType<PropertyFrame>()
						.FirstOrDefault(f => f.PropertyInfo == propertyInfo);

					if (frame != null)
						_internalPanel.Children.Remove(frame);
				});
			}
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class ItemsSourceAttribute : Attribute
	{
		public Type Type { get; set; }
		
		public ItemsSourceAttribute(Type type)
		{
			Type = type;
		}
	}

	public interface IItemsSource
	{
		ItemCollection GetValues();
	}

	public class ItemCollection : List<Item>
	{
		public void Add(object value)
		{
			base.Add(new Item()
			{
				DisplayName = value.ToString(), 
				Value = value
			});
		}

		public void Add( object value, string displayName )
		{
			base.Add(new Item()
			{
				DisplayName = displayName, 
				Value = value
			});
		}
	}
	
	public class Item
	{
		public string DisplayName { get; set; }
		
		public object Value { get; set; }
	}
}