using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentSpawnAdded(SegmentSpawner segmentSpawner) : ValueChangedMessage<SegmentSpawner>(segmentSpawner);
public class SegmentSpawnRemoved(SegmentSpawner segmentSpawner) : ValueChangedMessage<SegmentSpawner>(segmentSpawner);

public class SegmentSpawnsReset();
public class SegmentSpawnsChanged(SegmentSpawns spawners) : ValueChangedMessage<SegmentSpawns>(spawners);

public class SegmentSpawns : ObservableObject
{
	public string Name => "(Spawns)";

	public ObservableCollection<LocationSegmentSpawner> Location { get; set; }
		= new ObservableCollection<LocationSegmentSpawner>();

	public ObservableCollection<RegionSegmentSpawner> Region { get; set; }
		= new ObservableCollection<RegionSegmentSpawner>();

	public SegmentSpawns()
	{
		Location.CollectionChanged += OnCollectionChanged;
		Region.CollectionChanged += OnCollectionChanged;
	}

	private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
	{
		if (args.NewItems != null)
		{
			foreach (var newItem in args.NewItems.OfType<SegmentSpawner>())
				WeakReferenceMessenger.Default.Send(new SegmentSpawnAdded(newItem));
		}
			
		if (args.OldItems != null)
		{
			foreach (var oldItem in args.OldItems.OfType<SegmentSpawner>())
				WeakReferenceMessenger.Default.Send(new SegmentSpawnRemoved(oldItem));
		}
		
		if (args.Action is NotifyCollectionChangedAction.Reset)
			WeakReferenceMessenger.Default.Send(new SegmentSpawnsReset());
		
		WeakReferenceMessenger.Default.Send(new SegmentSpawnsChanged(this));
	}

	public void Load(SegmentEntities entities, XElement element, Version version)
	{
		Location.Clear();
		Region.Clear();
		
		foreach (var spawnElement in element.Elements("spawn"))
		{
			var type = spawnElement.Attribute("type");
			var spawner = default(SegmentSpawner);
						
			if (type != null)
			{
				switch (type.Value)
				{
					case nameof(LocationSegmentSpawner) or "LocationSpawner": spawner = new LocationSegmentSpawner(spawnElement); break; 
					case nameof(RegionSegmentSpawner) or "RegionSpawner": spawner = new RegionSegmentSpawner(spawnElement); break;
				}
			}

			if (spawner != null)
			{
				foreach (var entryElement in spawnElement.Elements("entry"))
				{
					var entry = new SpawnEntry(entryElement);

					var entity = default(SegmentEntity);
					var entityName = (string)entryElement.Attribute("entity");

					if (!String.IsNullOrEmpty(entityName))
					{
						entity = entities.FirstOrDefault(
							e => String.Equals(e.Name,
								entityName, StringComparison.Ordinal));
					}

					entry.SegmentEntity = entity;

					if (entry.SegmentEntity != null)
						spawner.Entries.Add(entry);
					else
						throw new Exception($"Unable to load spawn entry '{entityName}'.");
				}
							
				if (spawner is LocationSegmentSpawner locationSpawner)
					Location.Add(locationSpawner);
				else if (spawner is RegionSegmentSpawner regionSpawner)
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

			element.Add(locationSpawner.GetSerializingElement());
		}

		foreach (var regionSpawner in Region)
		{
			if (regionSpawner.Entries.Count < 1)
			{
				MessageBox.Show($"Region Spawner:{regionSpawner.Name} {messageForBlankEntities}",
					"Region Spawner Save Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			element.Add(regionSpawner.GetSerializingElement());
		}
	}

	public IEnumerable<SegmentSpawner> GetSpawns()
	{
		foreach (var spawn in Region)
			yield return spawn;

		foreach (var spawn in Location)
			yield return spawn;
	}
	
	public IEnumerable<SegmentSpawner> GetSpawns(SegmentEntity entity)
	{
		foreach (var spawn in GetSpawns().Where(s => s.Entries.Any(e => e.SegmentEntity == entity)))
			yield return spawn;
	}

	public IEnumerable<SpawnEntry> GetSpawnEntries()
	{
		foreach (var entry in Region.SelectMany(s => s.Entries))
			yield return entry;

		foreach (var entry in Location.SelectMany(s => s.Entries))
			yield return entry;
	}

	public IEnumerable<SpawnEntry> GetSpawnEntries(SegmentEntity entity)
	{
		foreach (var entry in GetSpawnEntries().Where(e => e.SegmentEntity == entity))
			yield return entry;
	}
}