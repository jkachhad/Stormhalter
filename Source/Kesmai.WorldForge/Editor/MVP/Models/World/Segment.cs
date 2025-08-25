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
using Project = Kesmai.WorldForge.Editor.Project;

namespace Kesmai.WorldForge.Editor;

public class Segment : ObservableObject
{
        private string _name;
        private Script _internal;
        private Script _definition;
    private SegmentProject _project = new SegmentProject();

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

        public Script Definition
        {
                get => _definition;
                set => SetProperty(ref _definition, value);
        }

    public SegmentProject Project
    {
            get => _project;
            set => SetProperty(ref _project, value);
    }

        public NotifyingCollection<SegmentRegion> Regions => Project.Regions;
        public SegmentLocations Locations => Project.Locations;
        public SegmentSubregions Subregions => Project.Subregions;
        public SegmentEntities Entities => Project.Entities;
        public NotifyingCollection<Project.SegmentSpawn> Spawns => Project.Spawns;
        public NotifyingCollection<Project.SegmentTreasure> Treasures => Project.Treasures;
        public NotifyingCollection<Project.SegmentHoard> Hoards => Project.Hoards;

	public Segment()
	{
                Name = "Segment";

                Project.Regions.CollectionChanged += OnRegionsChanged;

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

		if (_definition is null)
			_definition = new Script("definition", true, String.Empty, String.Empty);

		var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
			
		if (provider.TryGetTemplate(typeof(SegmentInternalScriptTemplate), out var internalTemplate))
			_internal.Template = internalTemplate;
		
		if (provider.TryGetTemplate(typeof(SegmentDefinitionScriptTemplate), out var definitionTemplate))
			_definition.Template = definitionTemplate;
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
                Project.Locations = new SegmentLocations();
			
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

                if (SegmentProject.ReservedLocations.Contains(location.Name))
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
                                if (SegmentProject.ReservedLocations.Contains(location.Name))
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
		
	public void UpdateTiles()
	{
		foreach (var region in Regions)
			region.UpdateTiles();
	}
}