using System;
using System.Linq;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;

namespace Kesmai.WorldForge.Windows
{
	public class CheckBoxPropertyEditor : PropertyEditor
	{
		private CheckBox _internal;

		protected override void OnLoad()
		{
			base.OnLoad();
			
			Children.Add(_internal = new CheckBox()
			{
				Y = 5,
				Width = 150, Height = 15,
			});

			var parent = this.GetAncestors().OfType<PropertyFrame>().FirstOrDefault();

			if (parent != null)
			{
				var propertyInfo = parent.PropertyInfo;
				var source = parent.Source;

				_internal.IsChecked = (bool)propertyInfo.GetValue(source);
				_internal.Properties.Get<bool>(ToggleButton.IsCheckedPropertyId).Changed += (o, args) =>
				{
					propertyInfo.SetValue(source, args.NewValue, null);
				};
			}
			else
			{
				throw new Exception("Unable to find parent frame for PropertyEditor");
			}
		}
	}
}