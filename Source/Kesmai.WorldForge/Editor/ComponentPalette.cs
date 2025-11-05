using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
	
	private ObservableCollection<ComponentsCategory> _rootCategories;
	private ObservableCollection<ComponentsCategory> _allCategories;
	
	private ComponentsCategory _staticCategory;
	private ComponentsCategory _editorCategory;
	private ComponentsCategory _segmentCategory;
	
	private ComponentsCategory _segmentBrushCategory;
	private ComponentsCategory _segmentTemplateCategory;
	
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
		get => _rootCategories;
		set => SetProperty(ref _rootCategories, value);
	}
	
	public ComponentPalette()
	{
		_rootCategories = new ObservableCollection<ComponentsCategory>();
		_allCategories = new ObservableCollection<ComponentsCategory>();
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
		
		_rootCategories.Add(staticCategory);
		
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
		
		_rootCategories.Add(_segmentCategory);

		_segmentBrushCategory = new ComponentsCategory()
		{
			Name = "Brushes",
			IsRoot = false
		};

		_segmentCategory.Subcategories.Add(_segmentBrushCategory);
		
		_segmentTemplateCategory = new ComponentsCategory()
		{
			Name = "Templates",
			IsRoot = false
		};

		_segmentCategory.Subcategories.Add(_segmentTemplateCategory);
		
		_allCategories.Add(_staticCategory);
		_allCategories.Add(_editorCategory);
		_allCategories.Add(_segmentCategory);
		_allCategories.Add(_segmentBrushCategory);
		_allCategories.Add(_segmentTemplateCategory);
		
		// setup message registration for adding segment components.
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentAdded>(this, (r, m) =>
		{
			addSegmentComponent(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentRemoved>(this, (r, m) =>
		{
			deleteSegmentComponent(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentComponentChanged>(this, (r, m) =>
		{
			var segmentComponent = m.Value;	
			
			deleteSegmentComponent(segmentComponent);
			addSegmentComponent(segmentComponent);
		});

		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentBrushAdded>(this, (r, m) =>
		{
			addSegmentBrush(m.Value);
		});

		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentBrushRemoved>(this, (r, m) =>
		{
			deleteSegmentBrush(m.Value);
		});

		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentBrushChanged>(this, (r, m) =>
		{
			var brush = m.Value;

			deleteSegmentBrush(brush);
			addSegmentBrush(brush);
		});

		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentBrushesChanged>(this, (r, m) =>
		{
			refreshSegmentBrushes(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentTemplateAdded>(this, (r, m) =>
		{
			addSegmentTemplate(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentTemplateRemoved>(this, (r, m) =>
		{
			deleteSegmentTemplate(m.Value);
		});
		
		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentTemplateChanged>(this, (r, m) =>
		{
			var template = m.Value;
			
			deleteSegmentTemplate(template);
			addSegmentTemplate(template);
		});

		WeakReferenceMessenger.Default.Register<ComponentPalette, SegmentTemplatesChanged>(this, (r, m) =>
		{
			refreshSegmentTemplates(m.Value);
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

		void addSegmentBrush(SegmentBrush brush)
		{
			if (brush is null || _segmentBrushCategory is null)
				return;

			if (!_segmentBrushCategory.Components.Contains(brush))
				_segmentBrushCategory.Components.Add(brush);
		}

		void deleteSegmentBrush(SegmentBrush brush)
		{
			if (brush is null || _segmentBrushCategory is null)
				return;

			_segmentBrushCategory.Components.Remove(brush);
		}

		void refreshSegmentBrushes(SegmentBrushes brushes)
		{
			if (_segmentBrushCategory is null)
				return;

			_segmentBrushCategory.Components.Clear();

			if (brushes is null)
				return;

			foreach (var brush in brushes)
				addSegmentBrush(brush);
		}

		void addSegmentTemplate(SegmentTemplate template)
		{
			if (template is null || _segmentTemplateCategory is null)
				return;

			if (!_segmentTemplateCategory.Components.Contains(template))
				_segmentTemplateCategory.Components.Add(template);
		}

		void deleteSegmentTemplate(SegmentTemplate template)
		{
			if (template is null || _segmentTemplateCategory is null)
				return;

			_segmentTemplateCategory.Components.Remove(template);
		}

		void refreshSegmentTemplates(SegmentTemplates templates)
		{
			if (_segmentTemplateCategory is null)
				return;

			_segmentTemplateCategory.Components.Clear();

			if (templates is null)
				return;

			foreach (var template in templates)
				addSegmentTemplate(template);
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
			
			_rootCategories.Add(category);
			_allCategories.Add(category);
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

				_allCategories.Add(subcategory);
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
		
		foreach (var searchCategory in _allCategories)
		{
			if (searchCategory.Components.Contains(component))
			{
				category = searchCategory;
				return true;
			}
		}

		return false;
	}
	
	private Dictionary<string, Type> _componentTypeCache = new Dictionary<string, Type>();
	private Dictionary<Type, ConstructorInfo> _componentCtorCache = new Dictionary<Type, ConstructorInfo>();
	
	public bool TryGetComponent(XElement element, out IComponentProvider component)
	{
		component = null;
		
		var nameAttribute = element.Attribute("name");

		if (nameAttribute is not null)
		{
			foreach (var searchCategory in _allCategories)
			{
				foreach (var childComponent in searchCategory.Components)
				{
					if (childComponent.Name.Equals(nameAttribute.Value, StringComparison.OrdinalIgnoreCase))
					{
						component = childComponent;
						return true;
					}
				}
			}
		}

		// the component was not found. determine if it's a terrain component.
		if (Equals(element.Name.LocalName, "component"))
		{
			var type = typeof(StaticComponent);
			var typeAttribute = element.Attribute("type");

			if (typeAttribute != null)
			{
				var typeName = $"Kesmai.WorldForge.Models.{typeAttribute.Value}";

				if (!_componentTypeCache.TryGetValue(typeName, out type))
				{
					type = Type.GetType(typeName);
					
					if (type != null)
						_componentTypeCache[typeName] = type;
				}
			}

			if (type is not null)
			{
				if (!_componentCtorCache.TryGetValue(type, out var ctor))
				{
					ctor = type.GetConstructor([typeof(XElement)]);
				
					if (ctor != null)
						_componentCtorCache[type] = ctor;
				}
			
				if (ctor is not null)
					component = ctor.Invoke([element]) as TerrainComponent;
			}
		}

		return (component is not null);
	}
}
