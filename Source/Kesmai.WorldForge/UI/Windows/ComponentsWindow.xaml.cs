using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CommonServiceLocator;
using Kesmai.WorldForge;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge.UI.Windows;

public partial class ComponentsWindow : Window
{
	private readonly ComponentPalette _componentPalette;
	private readonly HashSet<ComponentsCategory> _filteredCategories = new();

	private Predicate<IComponentProvider> _componentFilter;

	public IComponentProvider SelectedComponent { get; private set; }

	public Predicate<IComponentProvider> ComponentFilter
	{
		get => _componentFilter;
		set
		{
			if (_componentFilter != value)
			{
				_componentFilter = value;

				if (_componentFilter is null)
					ClearFilters();
				else
					ApplyFilter(_componentPalette.SelectedCategory);
			}
		}
	}

	public ComponentsWindow()
	{
		InitializeComponent();

		_componentPalette = ServiceLocator.Current.GetInstance<ComponentPalette>();
		_componentPalette.PropertyChanged += OnPalettePropertyChanged;

		Closed += OnClosed;
	}

	private void OnCategorySelected(object sender, RoutedPropertyChangedEventArgs<object> args)
	{
		if (args.NewValue is not ComponentsCategory category) 
			return;
		
		_componentPalette.SelectedCategory = category;
			
		ApplyFilter(category);
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

	private void OnPalettePropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(ComponentPalette.SelectedCategory))
			ApplyFilter(_componentPalette.SelectedCategory);

		if (e.PropertyName == nameof(ComponentPalette.SelectedProvider))
		{
			if (_componentFilter is not null)
				ValidateSelection();
		}
	}

	private void ApplyFilter(ComponentsCategory? category)
	{
		if (category is null)
			return;

		var view = CollectionViewSource.GetDefaultView(category.Components);

		if (view is null)
			return;

		if (_componentFilter is null)
		{
			if (view.Filter is not null)
				view.Filter = null;

			view.Refresh();
			
			_filteredCategories.Remove(category);

			return;
		}

		_filteredCategories.Add(category);
		
		view.Filter = FilterComponent;
		view.Refresh();

		ValidateSelection();
	}

	private void ValidateSelection()
	{
		if (_componentPalette.SelectedProvider is not null && !_componentFilter(_componentPalette.SelectedProvider))
			_componentPalette.SelectedProvider = null;
	}

	private bool FilterComponent(object item)
	{
		if (item is not IComponentProvider provider)
			return false;

		return _componentFilter is null || _componentFilter(provider);
	}

	private void ClearFilters()
	{
		if (_filteredCategories.Count is 0)
			return;

		var categories = new ComponentsCategory[_filteredCategories.Count];
		
		_filteredCategories.CopyTo(categories);

		foreach (var category in categories)
		{
			var view = CollectionViewSource.GetDefaultView(category.Components);

			if (view is null)
				continue;

			view.Filter = null;
			view.Refresh();
		}

		_filteredCategories.Clear();
	}

	private void OnClosed(object? sender, EventArgs e)
	{
		_componentPalette.PropertyChanged -= OnPalettePropertyChanged;
		
		ClearFilters();
		
		_componentFilter = null;
	}
}
