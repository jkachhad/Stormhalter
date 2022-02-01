using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Storages;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.UI
{
	public partial class ComponentsPanel : UserControl
	{
		public ComponentsPanel()
		{
			InitializeComponent();
		}

		public override void EndInit()
		{
			base.EndInit();
			
			var services = ServiceLocator.Current;
			
			var storage = services.GetInstance<IStorage>();
			var assembly = Assembly.GetExecutingAssembly();
			
			var collection = new ObservableCollection<ComponentsCategory>();
			
			/* Static Category */
			var staticCategory = new ComponentsCategory()
			{
				Name = "Static",
			};
			
			var terrainDocument = XDocument.Load(storage.OpenFile(@"Data\Terrain.xml"));
			var terrainRootElement = terrainDocument.Root;

			if (terrainRootElement != null)
			{
				foreach (var terrainElement in terrainRootElement.Elements("terrain"))
				{
					var idAttribute = terrainElement.Attribute("id");

					if (idAttribute != null)
					{
						staticCategory.Components.Add(new StaticComponent((int)idAttribute)
						{
							Name = idAttribute.Value
						});
					}
				}
			}

			try // Try needed until Terrain-External is everywhere WorldForge is.
			{
				terrainDocument = XDocument.Load(storage.OpenFile(@"Data\Terrain-External.xml"));
				terrainRootElement = terrainDocument.Root;

				if (terrainRootElement != null)
				{
					foreach (var terrainElement in terrainRootElement.Elements("terrain"))
					{
						var idAttribute = terrainElement.Attribute("id");

						if (idAttribute != null)
						{
							staticCategory.Components.Add(new StaticComponent((int)idAttribute)
							{
								Name = idAttribute.Value
							});
						}
					}
				}
			}
			catch (System.IO.FileNotFoundException) { }

			collection.Add(staticCategory);
			
			/* Other Categories */
			var document = Core.ComponentsResource;

			if (document != null)
			{
				var componentsRootElement = document.Root;

				if (componentsRootElement != null)
				{
					foreach (var categoryElement in componentsRootElement.Elements("category"))
					{
						var category = new ComponentsCategory()
						{
							Name = (string)categoryElement.Attribute("name"),
						};

						foreach (var element in categoryElement.Elements())
						{
							var componentTypename = $"Kesmai.WorldForge.Models.{element.Name}";
							var componentType = assembly.GetType(componentTypename, true);

							if (Activator.CreateInstance(componentType, element) is TerrainComponent component)
								category.Components.Add(component);
						}

						collection.Add(category);
					}
				}
			}

			_categories.ItemsSource = collection;
			
			if (_categories.Items.Count > 0)
				_categories.SelectedIndex = 0;
		}
	}
}