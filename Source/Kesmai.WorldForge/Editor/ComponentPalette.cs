using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CommonServiceLocator;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.UI;

namespace Kesmai.WorldForge;

public class ComponentPalette : ObservableRecipient
{
	private ObservableCollection<ComponentsCategory> _categories;

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

		foreach (var categoryElement in rootElement.Elements("category"))
		{
			var nameAttribute = categoryElement.Attribute("name");

			if (nameAttribute is null)
				throw new XmlException("Category element is missing name attribute.");

			var subcategory = category.Subcategories.FirstOrDefault(
				sub => sub.Name.Equals(nameAttribute.Value, StringComparison.OrdinalIgnoreCase));

			if (subcategory is null)
			{
				subcategory = new ComponentsCategory()
				{
					Name = nameAttribute.Value
				};

				category.Subcategories.Add(subcategory);
			}
			else
			{
				subcategory.Components.Clear();
			}
			
			subcategory.IsRoot = false;
			
			foreach (var componentElement in categoryElement.Elements())
			{
				try
				{
					var componentTypename = $"Kesmai.WorldForge.Models.{componentElement.Name}";
					var componentType = Type.GetType(componentTypename);

					if (componentType is null)
						throw new XmlException($"Component type '{componentTypename}' not found.");

					var ctor = componentType.GetConstructor([typeof(XElement)]);

					if (ctor is null)
						throw new XmlException(
							$"Component type '{componentTypename}' is missing constructor with XElement parameter.");

					var component = ctor.Invoke([componentElement]) as TerrainComponent;

					if (component is null)
						throw new XmlException($"Component type '{componentTypename}' failed to instantiate.");

					subcategory.Components.Add(component);
				}
				catch (Exception exception)
				{
					System.Windows.MessageBox.Show($"Component failed to parse:\n${componentElement}\n{exception.Message}",
						"Custom Components.xml failed to load", System.Windows.MessageBoxButton.OK);
				}
			}
		}
	}
	
	public bool TryGetCategory(string name, out ComponentsCategory category)
	{
		return (category = Categories.FirstOrDefault(
			c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) != null;
	}
}
