using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI;

public class ComponentsCategory : ObservableObject
{
	public string Name { get; set; }

	private ObservableCollection<TerrainComponent> _components;
	private ObservableCollection<ComponentsCategory> _subcategories;
	
	private bool _isRoot;
	
	public ObservableCollection<TerrainComponent> Components
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
		_components = new ObservableCollection<TerrainComponent>();
		_subcategories = new ObservableCollection<ComponentsCategory>();
	}
}