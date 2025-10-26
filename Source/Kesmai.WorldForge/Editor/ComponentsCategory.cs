using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class ComponentsCategory : ObservableObject
{
	public string Name { get; set; }

	private ObservableCollection<IComponentProvider> _components;
	private ObservableCollection<ComponentsCategory> _subcategories;
	
	private bool _isSelected;
	private bool _isExpanded;
	
	public bool IsSelected
	{
		get => _isSelected;
		set => SetProperty(ref _isSelected, value);
	}
	
	public bool IsExpanded
	{
		get => _isExpanded;
		set => SetProperty(ref _isExpanded, value);
	}
	
	private bool _isRoot;
	
	public ObservableCollection<IComponentProvider> Components
	{
		get => _components;
		set => SetProperty(ref _components, value);
	}
	
	public ObservableCollection<ComponentsCategory> Subcategories
	{
		get => _subcategories;
		set => SetProperty(ref _subcategories, value);
	}

	public bool IsRoot
	{
		get => _isRoot;
		set => SetProperty(ref _isRoot, value);
	}

	public ComponentsCategory()
	{
		_components = new ObservableCollection<IComponentProvider>();
		_subcategories = new ObservableCollection<ComponentsCategory>();
	}
	
	public bool TryGetCategory(string name, out ComponentsCategory category)
	{
		return (category = _subcategories.FirstOrDefault(
			c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) != null;
	}
	
	public bool TryGetComponent(string name, out IComponentProvider component)
	{
		return (component = _components.FirstOrDefault(
			c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) != null;
	}
}