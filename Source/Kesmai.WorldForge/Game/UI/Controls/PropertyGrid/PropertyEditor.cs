using System;
using System.Linq;
using System.Reflection;
using DigitalRune.Game.UI;
using DigitalRune.Game.UI.Controls;

namespace Kesmai.WorldForge.Windows;

public abstract class PropertyEditor : Canvas
{
	protected PropertyGrid GetPropertyGrid()
	{
		return this.GetAncestors().OfType<PropertyGrid>().Single();
	}
	
	protected void NotifyPropertyChanged(PropertyInfo property)
	{
		var propertyGrid = GetPropertyGrid();

		if (propertyGrid is null)
			throw new ArgumentNullException(nameof(propertyGrid));
		
		propertyGrid.NotifyPropertyChanged(property);
	}
}