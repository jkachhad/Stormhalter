using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DigitalRune.Game;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows
{
	public class PropertyFrame : StackPanel
	{
		private TextBlock _nameBlock;

		public static readonly int PropertyInfoPropertyId = CreateProperty(
			typeof(PropertyFrame), "PropertyInfo", GamePropertyCategories.Default, null, default(PropertyInfo),
			UIPropertyOptions.AffectsRender);

		public PropertyInfo PropertyInfo
		{
			get => GetValue<PropertyInfo>(PropertyInfoPropertyId);
			set => SetValue(PropertyInfoPropertyId, value);
		}
		
		public static readonly int ItemPropertyId = CreateProperty(
			typeof(PropertyFrame), "Item", GamePropertyCategories.Default, null, default(object),
			UIPropertyOptions.AffectsRender);

		public object Source
		{
			get => GetValue<object>(ItemPropertyId);
			set => SetValue(ItemPropertyId, value);
		}
		
		public PropertyFrame()
		{
			Orientation = Orientation.Horizontal;

			HorizontalAlignment = HorizontalAlignment.Stretch;
			
			Style = "DarkCanvas";
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			var nameCanvas = new Canvas()
			{
				Width = 150,
				VerticalAlignment = VerticalAlignment.Center,
			};
			_nameBlock = new TextBlock()
			{
				Foreground = Color.Yellow,
				Shadow = Color.Black,
				
				Font = "Tahoma14Bold",
				
				HorizontalAlignment = HorizontalAlignment.Right,
				
				Margin = new Vector4F(0, 0, 5, 0)
			};
			nameCanvas.Children.Add(_nameBlock);
			
			var editorCanvas = new Canvas()
			{
				VerticalAlignment = VerticalAlignment.Center,
			};

			var editor = GetEditor();

			if (editor != null)
				editorCanvas.Children.Add(editor);
			
			_nameBlock.Text = PropertyInfo.Name;

			Children.Add(nameCanvas);
			Children.Add(editorCanvas);
			
			Properties.Get<PropertyInfo>(PropertyInfoPropertyId).Changed += (o, args) =>
			{
				_nameBlock.Text = args.NewValue.Name;
			};
		}

		protected override void OnHandleInput(InputContext context)
		{
			if (!IsLoaded || !IsVisible)
				return;
			
			base.OnHandleInput(context);

			var inputService = InputService;

			if (inputService.IsKeyboardHandled)
				return;
		}

		private static Type _enumType = typeof(Enum);

		private PropertyEditor GetEditor()
		{
			var propertyInfo = PropertyInfo;
			var propertyType = propertyInfo.PropertyType;

			if (propertyType == typeof(Color))
				return new ColorPropertyEditor();
			if (propertyType == typeof(bool))
				return new CheckBoxPropertyEditor();
			if (_enumType.IsAssignableFrom(propertyType))
				return new EnumPropertyEditor();
			
			var itemsSourceAttribute = propertyInfo.GetCustomAttribute<ItemsSourceAttribute>();

			if (itemsSourceAttribute != null)
				return new CheckComboBoxPropertyEditor(itemsSourceAttribute);

			
			var converter = TypeDescriptor.GetConverter(propertyType);

			if (converter.CanConvertFrom(typeof(string)))
			{
				bool isMultiLine = propertyInfo.Name == "Comment";
				bool canBeNull = propertyInfo.Name == "Comment";
				return new TextBoxPropertyEditor(isMultiLine, canBeNull);				
			}

			return default(PropertyEditor);
		}
	}
}