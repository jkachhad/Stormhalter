using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Ionic.Zip;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class Segment : ObservableObject
{
	private static List<string> _reservedLocations = new List<string>()
	{
		"Entrance", "Resurrect", "Facet", "Thief", 
	};
		
	private string _name;
	private Script _internal;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}
		
	public Script Internal
	{
		get => _internal;
		set => SetProperty(ref _internal, value);
	}

	public NotifyingCollection<SegmentRegion> Regions { get; set; } = new NotifyingCollection<SegmentRegion>();
	public SegmentLocations Locations { get; set; } = new SegmentLocations();
	public SegmentSubregions Subregions { get; set; } = new SegmentSubregions();
	public SegmentEntities Entities { get; set; } = new SegmentEntities();
	public SegmentSpawns Spawns { get; set; } = new SegmentSpawns();
	public SegmentTreasures Treasures { get; set; } = new SegmentTreasures();

	public Segment()
	{
		Name = "Segment";

		foreach (var location in _reservedLocations)
		{
			Locations.Add(new SegmentLocation()
			{
				Name = location
			});
		}
			
		Regions = new NotifyingCollection<SegmentRegion>();
		Regions.CollectionChanged += OnRegionsChanged;

		ValidateScripts();
	}
		
	private void ValidateScripts()
	{
		if (_internal == null)
		{
			_internal = new Script("Internal", true,
				String.Empty,
				"\n",
				String.Empty
			);
		}
			
		var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
			
		if (provider.TryGetTemplate(typeof(SegmentScriptTemplate), out var template))
			_internal.Template = template;
	}

	private void OnRegionsChanged(object sender, CollectionChangedEventArgs<SegmentRegion> args)
	{
		var updateNew = (args.Action == CollectionChangedAction.Add);
		var updateOld = (args.Action == CollectionChangedAction.Remove || args.Action == CollectionChangedAction.Clear);

		if (args.Action == CollectionChangedAction.Replace)
			updateNew = updateOld = true;

		var services = ServiceLocator.Current;
		var presenter = services.GetInstance<ApplicationPresenter>();

		if (updateNew)
			args.NewItems.ForEach(region => { presenter.Documents.Add(region); });

		if (updateOld)
			args.OldItems.ForEach(region => { presenter.Documents.Remove(region); });
	}
		
		
	public void Load(XElement element)
	{
		Name = (string)element.Attribute("name");
		Locations = new SegmentLocations();
			
		var versionAttribute = element.Attribute("version");

		if (versionAttribute != null)
		{
			Load(element, new Version((string)versionAttribute));
			return;
		}
			
		/* Loading deprecated formats. */
		var regions = new List<SegmentRegion>();

		foreach (var regionElement in element.Elements("region"))
			regions.Add(new SegmentRegion(regionElement));
		
		if (regions.Count > 0)
			Regions.AddRange(regions);
		
		var locationsElement = element.Element("locations");

		if (locationsElement != null)
		{
			foreach (var locationElement in locationsElement.Elements("location"))
			{
				var location = new SegmentLocation(locationElement);

				if (_reservedLocations.Contains(location.Name))
					location.IsReserved = true;

				Locations.Add(location);
			}
		}
	}

	public void Load(XElement element, Version version)
	{
		var regionsElement = element.Element("regions");
		var locationsElement = element.Element("locations");
		var subregionsElement = element.Element("subregions");
		var entitiesElement = element.Element("entities");
		var spawnsElement = element.Element("spawns");
		var treasuresElement = element.Element("treasures");
		var hoardsElement = element.Element("hoards");
		var scriptsElements = element.Elements("script").ToList();
			
		if (regionsElement != null)
		{
			foreach (var regionElement in regionsElement.Elements("region"))
				Regions.Add(new SegmentRegion(regionElement));
		}
	
		if (locationsElement != null)
		{
			Locations.Load(locationsElement, version);

			foreach (var location in Locations)
			{
				if (_reservedLocations.Contains(location.Name))
					location.IsReserved = true;
			}
		}

		if (scriptsElements.Any())
		{
			var internalScriptElement = scriptsElements
				.FirstOrDefault(e => String.Equals(e.Attribute("name")?.Value, "Internal", StringComparison.Ordinal));

			if (internalScriptElement != null)
			{
				_internal = new Script(internalScriptElement);
			}
			else
			{
				_internal = new Script("Internal", true,
					String.Empty,
					"\n",
					String.Empty
				);
			}
		}

		if (subregionsElement != null)
			Subregions.Load(subregionsElement, version);

		if (entitiesElement != null)
			Entities.Load(entitiesElement, version);

		if (spawnsElement != null)
			Spawns.Load(Entities, spawnsElement, version);

		if (treasuresElement != null)
			Treasures.Load(treasuresElement, version);
			
		ValidateScripts();
	}

	public SegmentRegion GetRegion(int id)
	{
		return Regions.FirstOrDefault(region => region.ID == id);
	}
		
#if (ArchiveStorage)
		public void Save(ZipFile archive)
		{
			#region Segment
			
			var segmentDocument = new XDocument();
			var segmentElement = GetXElement();

			segmentDocument.Add(segmentElement);
			
			var memoryStream = new MemoryStream();
			var streamWriter = new StreamWriter(memoryStream);

			segmentDocument.Save(streamWriter);

			memoryStream.Position = 0;

			archive.AddEntry(@"regions", memoryStream);
			
			#endregion

			Locations.Save(archive);
			Subregions.Save(archive);
			Entities.Save(archive);
			Spawns.Save(archive);
		}
#else
	public void Save(XElement element)
	{
		var regionsElement = new XElement("regions");
		var locationsElement = new XElement("locations");
		var subregionsElement = new XElement("subregions");
		var entitiesElement = new XElement("entities");
		var spawnsElement = new XElement("spawns");
		var treasuresElement = new XElement("treasures");
		var hoardsElement = new XElement("hoards");
			
		#region Regions
			
		foreach (var region in Regions)
			regionsElement.Add(region.GetXElement());

		#endregion
			
		#region Locations

		Locations.Save(locationsElement);

		#endregion
			
		#region Subregions

		Subregions.Save(subregionsElement);

		#endregion
			
		#region Entities

		Entities.Save(entitiesElement);

		#endregion
			
		#region Spawns

		Spawns.Save(spawnsElement);

		#endregion
			
		#region Treasures

		Treasures.Save(treasuresElement);
			
		#endregion

		element.Add(_internal.GetXElement());
			
		element.Add(regionsElement);
		element.Add(locationsElement);
		element.Add(subregionsElement);
		element.Add(entitiesElement);
		element.Add(spawnsElement);
		element.Add(treasuresElement);
		element.Add(hoardsElement);
	}
#endif
	
	/*[Deprecated]*/
	public void Load(ZipFile archive)
	{
		var version = new Version(archive.Comment);
		var components = archive["regions"];
			
		var stream = XDocument.Load(components.OpenReader());
		var element = stream.Root;
			
		Name = (string)element.Element("name");
			
		Locations = new SegmentLocations();

		foreach (var regionElement in element.Elements("region"))
			Regions.Add(new SegmentRegion(regionElement));

		/* Locations */
		if (version.Minor < 56)
		{
			var locationsElement = element.Element("locations");

			if (locationsElement != null)
			{
				foreach (var locationElement in locationsElement.Elements("location"))
					Locations.Add(new SegmentLocation(locationElement));
			}
		}
		else
		{
			Locations.Load(archive, version);

			foreach (var location in Locations)
			{
				if (_reservedLocations.Contains(location.Name))
					location.IsReserved = true;
			}
		}

		/* Subregions */
		Subregions.Load(archive, version);
			
		/* Entities */
		Entities.Load(archive, version);
			
		/* Spawns */
		Spawns.Load(Entities, archive, version);
	}
		
	public void UpdateTiles()
	{
		foreach (var region in Regions)
			region.UpdateTiles();
	}
}