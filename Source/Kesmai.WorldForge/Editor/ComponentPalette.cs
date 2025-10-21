using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.UI;

namespace Kesmai.WorldForge;

public class ComponentPalette : ObservableRecipient
{
	private ComponentsCategory _selectedCategory;
	private IComponentProvider _selectedProvider;
	
	private ObservableCollection<ComponentsCategory> _categories;
	
	private ComponentsCategory _staticCategory;
	private ComponentsCategory _editorCategory;
	private ComponentsCategory _segmentCategory;
	
	public IComponentProvider SelectedProvider
	{
		get => _selectedProvider;
		set => SetProperty(ref _selectedProvider, value);
	}
	
	public ComponentsCategory SelectedCategory
	{
		get => _selectedCategory;
		set => SetProperty(ref _selectedCategory, value);
	}

	public ObservableCollection<ComponentsCategory> Categories
	{
		get => _categories;
		set => SetProperty(ref _categories, value);
	}
	
	public ComponentPalette()
	{
		_categories = new ObservableCollection<ComponentsCategory>();
	}

	public void Initialize()
	{
		// get the terrain manager.
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager is null)
			throw new ArgumentNullException(nameof(terrainManager));
		
		// load static component from terrain.
		var staticCategory = new ComponentsCategory
		{
			Name = "STATIC",
			IsRoot = true
		};

		foreach (var (_, terrain) in terrainManager)
		{
			var component = new StaticComponent(terrain.ID)
			{
				Name = terrain.ID.ToString(),
			};

			staticCategory.Components.Add(component);
		}
		
		_categories.Add(staticCategory);
		
		// load editor components
		Task.Run(async () =>
		{
			var packageReader = await NuGetResolver.Resolve("Kesmai.Server.Reference", "net8.0-windows8.0");
			var documentStream = await NuGetResolver.ResolveStream(packageReader, "Components.xml");

			Load("EDITOR", XDocument.Load(documentStream));
		});

		_staticCategory = staticCategory;

		if (TryGetCategory("EDITOR", out var editorCategory))
			_editorCategory = editorCategory;

		// create segment category
		_segmentCategory = new ComponentsCategory()
		{
			Name = "SEGMENT",
			IsRoot = true
		};
		
		_categories.Add(_segmentCategory);
		
		// setup message registration for adding segment components.
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentCreated>(this, (r, m) =>
		{
			addSegmentComponent(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentDeleted>(this, (r, m) =>
		{
			deleteSegmentComponent(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentChanged>(this, (r, m) =>
		{
			var segmentComponent = m.Value;	
			
			deleteSegmentComponent(segmentComponent);
			addSegmentComponent(segmentComponent);
		});
		
		void addSegmentComponent(SegmentComponent component)
		{
			if (component.Element is null)
				return;
			
			var pathAttribute = component.Element.Attribute("category");
			var category = default(ComponentsCategory);
			
			if (pathAttribute is not null)
				category = TryGetCategory(pathAttribute.Value, _segmentCategory);

			category ??= _segmentCategory;

			if (!category.Components.Contains(component))
				category.Components.Add(component);
		}
		
		void deleteSegmentComponent(SegmentComponent component)
		{
			if (TryGetCategory(component, out var category))
				category.Components.Remove(component);
		}
	}

	public void Load(string categoryName, XDocument document)
	{
		// load components from xml document
		if (!TryGetCategory(categoryName, out var category))
		{
			category = new ComponentsCategory()
			{
				Name = categoryName,
				IsRoot = true
			};
			
			_categories.Add(category);
		}
		else
		{
			category.IsRoot = true;
		}

		var rootElement = document.Root;

		if (rootElement is null)
			throw new XmlException("Components.xml is missing root element.");
		
		foreach (var componentElement in rootElement.Elements())
		{
			try
			{
				var componentTypeAttribute = componentElement.Attribute("type");
				var componentCategoryAttribute = componentElement.Attribute("category");
					
				if (componentTypeAttribute is null)
					throw new XmlException("Component element is missing type attribute.");

				if (componentCategoryAttribute is null)
					throw new XmlException("Component element is missing category attribute.");

				var categoryPath = componentCategoryAttribute.Value;
				var componentsCategory = TryGetCategory(categoryPath, category);
					
				if (componentsCategory is null)
					throw new XmlException($"Component category '{categoryPath}' not found.");
					
				var componentTypename = $"Kesmai.WorldForge.Models.{componentTypeAttribute.Value}";
				var componentType = Type.GetType(componentTypename);

				if (componentType is null)
					throw new XmlException($"Component type '{componentTypename}' not found.");

				var ctor = componentType.GetConstructor([typeof(XElement)]);

				if (ctor is null)
					throw new XmlException($"Component type '{componentTypename}' is missing constructor with XElement parameter.");

				var component = ctor.Invoke([componentElement]) as TerrainComponent;

				if (component is null)
					throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");

				componentsCategory.Components.Add(component);
			}
			catch (Exception exception)
			{
				System.Windows.MessageBox.Show($"Component failed to parse:\n${componentElement}\n{exception.Message}",
					"Custom Components.xml failed to load", System.Windows.MessageBoxButton.OK);
			}
		}
	}

	public ComponentsCategory TryGetCategory(string categoryPath, ComponentsCategory category = null)
	{
		var parts = categoryPath.Split([':'], StringSplitOptions.RemoveEmptyEntries);

		if (category is null)
			category = _editorCategory;
		
		for (var i = 0; i < parts.Length; i++)
		{
			var categoryName = parts[i];

			if (!category.TryGetCategory(categoryName, out var subcategory))
			{
				subcategory = new ComponentsCategory()
				{
					Name = categoryName,
					IsRoot = false,
				};
				category.Subcategories.Add(subcategory);
			}
			
			category = subcategory;
		}
		
		return category;
	}
	
	public bool TryGetCategory(string name, out ComponentsCategory category)
	{
		return (category = Categories.FirstOrDefault(
			c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) != null;
	}

	public bool TryGetCategory(SegmentComponent component, out ComponentsCategory category)
	{
		category = null;
		
		foreach (var parentCategory in _categories)
		{
			if (recursive(component, parentCategory, out category))
				return true;
		}

		return false;
		
		bool recursive(SegmentComponent source, ComponentsCategory search, out ComponentsCategory targetCategory)
		{
			if (search.Components.Contains(source))
			{
				targetCategory = search;
				return true;
			}

			if (search.Subcategories.Count is not 0)
			{
				foreach (var child in search.Subcategories)
				{
					if (recursive(source, child, out targetCategory))
						return true;
				}
			}

			targetCategory = null;
			return false;
		}
	}
}
