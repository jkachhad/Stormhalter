#nullable enable

using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Kesmai.WorldForge.UI;

/// <summary>
/// A grid column edited with a dropdown that also accepts typed text, so a value read from a script still shows
/// (and is kept) when it isn't one of the choices.
/// </summary>
public class ChoiceColumn : DataGridBoundColumn
{
	public IEnumerable? ItemsSource { get; set; }

	protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
	{
		var text = new TextBlock { Margin = new Thickness(2, 0, 2, 0) };

		if (Binding != null)
			BindingOperations.SetBinding(text, TextBlock.TextProperty, Binding);

		return text;
	}

	protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
	{
		var combo = new ComboBox { IsEditable = true, IsTextSearchEnabled = true, ItemsSource = ItemsSource, Margin = new Thickness(0) };

		if (Binding is Binding binding)
			BindingOperations.SetBinding(combo, ComboBox.TextProperty, new Binding(binding.Path.Path) { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

		return combo;
	}

	protected override object? PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
	{
		editingElement.Focus();
		return null;
	}
}
