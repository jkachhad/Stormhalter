using System;
using System.ComponentModel;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Windows
{
	public class TextBoxPropertyEditor : PropertyEditor
	{
		private TextBox _internal;
		private bool _multiline = false;
		private bool _canBeNull;

		public TextBoxPropertyEditor(bool multiline, bool canBeNull) : base ()
        {
			_multiline = multiline;
			_canBeNull = canBeNull;
        }
		protected override void OnLoad()
		{
			base.OnLoad();
			
			Children.Add(_internal = new TextBox()
			{
				Width = 150,
				
				Font = "Tahoma14Bold",
				Foreground = Color.Black,
			});

			if (_multiline)
				_internal.MaxLines = 10;

			var parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

			if (parent != null)
			{
				var propertyInfo = parent.PropertyInfo;
				var source = parent.Source;

				var value = propertyInfo.GetValue(source);
				
				if (value != null)
					_internal.Text = value.ToString();
				
				_internal.Properties.Get<string>(TextBox.TextPropertyId).Changed += (o, args) =>
				{
					var color = Color.Black;
					var converter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
					var value = args.NewValue;

					try
					{
						if ((!String.IsNullOrEmpty(value) || _canBeNull) && converter.CanConvertFrom(typeof(string)))
							propertyInfo.SetValue(source, converter.ConvertFrom(value), null);
						else
							color = Color.Red;
					}
					catch
					{
					}

					_internal.Foreground = color;
				};
			}
			else
			{
				throw new Exception("Unable to find parent frame for PropertyEditor");
			}
		}

	}
}