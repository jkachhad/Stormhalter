#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// Property grid editor for a subregion name: a dropdown of the open segment's subregions, sorted by region then name,
/// that also takes a typed name.
/// </summary>
public class SubregionNameEditor : ITypeEditor
{
	public FrameworkElement ResolveEditor(PropertyItem propertyItem)
	{
		// no open segment (or nobody to ask) just leaves the list empty; a name can still be typed
		var request = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
		var segment = request.HasReceivedResponse ? request.Response : null;

		var names = segment?.Subregions
			.OrderBy(s => s.Region).ThenBy(s => s.Name)
			.Select(s => s.Name)
			.Where(n => !String.IsNullOrWhiteSpace(n))
			.Distinct()
			.ToList() ?? [];

		var combo = new ComboBox { IsEditable = true, ItemsSource = names, IsTextSearchEnabled = true };

		combo.SetBinding(ComboBox.TextProperty, new Binding(nameof(PropertyItem.Value))
		{
			Source = propertyItem,
			Mode = BindingMode.TwoWay,
			UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
		});

		combo.SelectionChanged += (_, _) =>
		{
			if (combo.SelectedItem is string name)
				propertyItem.Value = name;
		};

		return combo;
	}
}
