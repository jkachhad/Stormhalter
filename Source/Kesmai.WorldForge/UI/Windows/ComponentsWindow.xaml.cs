using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Windows;

public partial class ComponentsWindow : Window
{
	private readonly ComponentPalette _componentPalette;

	public IComponentProvider? SelectedComponent { get; private set; }

	public ComponentsWindow()
	{
		InitializeComponent();

		if (Application.Current?.TryFindResource("componentPalette") is not ComponentPalette palette)
			throw new InvalidOperationException("Component palette resource is unavailable.");

		_componentPalette = palette;
	}

	private void OnCategorySelected(object sender, RoutedPropertyChangedEventArgs<object> args)
	{
		if (args.NewValue is ComponentsCategory category)
			_componentPalette.SelectedCategory = category;
	}

	private void OnSelectClicked(object sender, RoutedEventArgs e)
	{
		if (_componentPalette.SelectedProvider is null)
			return;

		SelectedComponent = _componentPalette.SelectedProvider;
		DialogResult = true;
	}

	private void OnComponentDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_componentPalette.SelectedProvider is null)
			return;

		SelectedComponent = _componentPalette.SelectedProvider;
		DialogResult = true;
	}
}
