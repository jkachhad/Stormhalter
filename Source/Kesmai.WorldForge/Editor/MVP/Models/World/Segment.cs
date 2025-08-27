using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge;
using Kesmai.WorldForge.Roslyn;
using RoslynPad.Roslyn;

namespace Kesmai.WorldForge.Editor;

public class Segment : ObservableObject, IDisposable
{
	private static List<string> _reservedLocations = new List<string>()
	{
		"Entrance", "Resurrect", "Facet", "Thief", 
	};
		
        private string _name;
        private string _rootPath;
    private SegmentWorkspace _workspace;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}
		
        public string RootPath
        {
                get => _rootPath;
                set
        {
            if (SetProperty(ref _rootPath, value))
            {
                Workspace?.Dispose();
                Workspace = null;
            }
        }
        }

    public void InitializeWorkspace(IRoslynHost host)
    {
        if (!string.IsNullOrEmpty(RootPath))
        {
            Workspace?.Dispose();
            Workspace = new SegmentWorkspace(RootPath, host);
        }
    }

    public SegmentWorkspace Workspace
    {
            get => _workspace;
            private set => SetProperty(ref _workspace, value);
    }
	
	public NotifyingCollection<SegmentRegion> Regions { get; } = new NotifyingCollection<SegmentRegion>();
	public SegmentLocations Locations { get; set; } = new SegmentLocations();
	public SegmentSubregions Subregions { get; set; } = new SegmentSubregions();
	public SegmentEntities Entities { get; set; } = new SegmentEntities();
	public SegmentSpawns Spawns { get; set; } = new SegmentSpawns();
	public SegmentTreasures Treasures { get; set; } = new SegmentTreasures();
	public NotifyingCollection<VirtualFile> VirtualFiles { get; } = new NotifyingCollection<VirtualFile>();

        public Segment()
        {
                Regions.CollectionChanged += OnRegionsChanged;
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

    public void Dispose()
    {
        Workspace?.Dispose();
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
                Regions.Clear();
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

		Regions.Clear();
			
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
		
		if (subregionsElement != null)
			Subregions.Load(subregionsElement, version);

		if (entitiesElement != null)
			Entities.Load(entitiesElement, version);

		if (spawnsElement != null)
			Spawns.Load(Entities, spawnsElement, version);

		if (treasuresElement != null)
			Treasures.Load(treasuresElement, version);
	}

	public SegmentRegion GetRegion(int id)
	{
		return Regions.FirstOrDefault(region => region.ID == id);
	}
	
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
		
		element.Add(regionsElement);
		element.Add(locationsElement);
		element.Add(subregionsElement);
		element.Add(entitiesElement);
		element.Add(spawnsElement);
                element.Add(treasuresElement);
                element.Add(hoardsElement);
        }
	
	public void UpdateTiles()
	{
		foreach (var region in Regions)
			region.UpdateTiles();
	}
}