using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class SegmentEntities : ObservableCollection<Entity>
{
	public void Load(XElement element, Version version)
	{
		foreach (var entityElement in element.Elements("entity"))
		{
			Add(new Entity(entityElement));
		}
	}
		
	public void Save(XElement element)
	{
		foreach (var entity in this)
		{
			var scriptsToString = entity.Scripts[0].ToString();

			if (scriptsToString.Contains("return new MobileEntity()"))
			{
				MessageBox.Show($"Make sure to add code for: {entity.Name}, otherwise compiliation errors will occur if you leave" +
				                $"{Environment.NewLine} return new MobileEntity(); in the code");

			}

			element.Add(entity.GetXElement());
		}
				
	}
}

public class SegmentSpawns : ObservableObject
{
	public string Name => "(Spawns)";

	public ObservableCollection<LocationSpawner> Location { get; set; }
		= new ObservableCollection<LocationSpawner>();

	public ObservableCollection<RegionSpawner> Region { get; set; }
		= new ObservableCollection<RegionSpawner>();
		
	public void Load(SegmentEntities entities, XElement element, Version version)
	{
		foreach (var spawnElement in element.Elements("spawn"))
		{
			var type = spawnElement.Attribute("type");
			var spawner = default(Spawner);
						
			if (type != null)
			{
				switch ((string)type)
				{
					case "LocationSpawner": spawner = new LocationSpawner(spawnElement); break; 
					case "RegionSpawner": spawner = new RegionSpawner(spawnElement); break;
				}
			}

			if (spawner != null)
			{
				foreach (var entryElement in spawnElement.Elements("entry"))
				{
					var entry = new SpawnEntry(entryElement);

					var entity = default(Entity);
					var entityName = (string)entryElement.Attribute("entity");

					if (!String.IsNullOrEmpty(entityName))
					{
						entity = entities.FirstOrDefault(
							e => String.Equals(e.Name,
								entityName, StringComparison.Ordinal));
					}

					entry.Entity = entity;

					if (entry.Entity != null)
						spawner.Entries.Add(entry);
					else
						throw new Exception($"Unable to load spawn entry '{entityName}'.");
				}
							
				if (spawner is LocationSpawner locationSpawner)
					Location.Add(locationSpawner);
				else if (spawner is RegionSpawner regionSpawner)
					Region.Add(regionSpawner);
			}
		}
	}

	public void Save(XElement element)
	{
		string messageForBlankEntities = $" has an entry that is blank. {Environment.NewLine} {Environment.NewLine}" +
		                                 $"Update prior to Checkin, otherwise compilation errors.";

		foreach (var locationSpawner in Location)
		{
			if (locationSpawner.Entries.Count < 1)
			{
				MessageBox.Show($"Location Spawner:{locationSpawner.Name} {messageForBlankEntities}", 
					"Location Spawner Save Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

			element.Add(locationSpawner.GetXElement());
		}

		foreach (var regionSpawner in Region)
		{
			if (regionSpawner.Entries.Count < 1)
			{
				MessageBox.Show($"Region Spawner:{regionSpawner.Name} {messageForBlankEntities}",
					"Region Spawner Save Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			element.Add(regionSpawner.GetXElement());
		}
	}
}