using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Models;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.Editor;

public interface ISegmentObject
{
	string Name { get; set; }
	
	void Present(ApplicationPresenter presenter);
}

public class Segment : ObservableObject, ISegmentObject
{
	private static List<string> _reservedLocations = new List<string>()
	{
		"Entrance", "Resurrect", "Facet", "Thief", 
	};
		
	private string _name;
	private string _directory;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}

	public string Directory
	{
		get => _directory;
		set => _directory = value;
	}
	
	public SegmentRegions Regions { get; set; } = new SegmentRegions();
	public SegmentLocations Locations { get; set; } = new SegmentLocations();
	public SegmentSubregions Subregions { get; set; } = new SegmentSubregions();
	public SegmentEntities Entities { get; set; } = new SegmentEntities();
	public SegmentSpawns Spawns { get; set; } = new SegmentSpawns();
	public SegmentTreasures Treasures { get; set; } = new SegmentTreasures();

	public Segment()
	{
		_name = "Segment";

		foreach (var location in _reservedLocations)
		{
			Locations.Add(new SegmentLocation()
			{
				Name = location
			});
		}
		
		// register for changes in files to update the segment
		WeakReferenceMessenger.Default.Register<SegmentFileChangedMessage>(this, (_, message) => OnSegmentFileChanged(message.Value));
	}

	public void Present(ApplicationPresenter presenter)
	{
		var segmentViewModel = presenter.Documents.OfType<SegmentViewModel>().FirstOrDefault();

		if (segmentViewModel is null)
			presenter.Documents.Add(segmentViewModel = new SegmentViewModel(presenter.Segment));

		if (presenter.ActiveDocument != segmentViewModel)
			presenter.SetActiveDocument(segmentViewModel);

		presenter.SetActiveContent(this);
		segmentViewModel.Segment = this;
	}
	
	public void OnSegmentFileChanged(FileSystemEventArgs args)
	{
		var path = args.FullPath;
		var extension = Path.GetExtension(path).ToLower();
		
		// we only process xml files.
		if (!extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
			return;
		
		var name = Path.GetFileNameWithoutExtension(path).ToLower();
		var document = XDocument.Load(path);
		var rootElement = document.Root;

		if (rootElement is null)
			return;

		try
		{
			switch (name)
			{
				case "locations": Locations.Load(rootElement, Core.Version); break;
				case "subregions": Subregions.Load(rootElement, Core.Version); break;
				case "entities": Entities.Load(rootElement, Core.Version); break;
				case "spawns": Spawns.Load(Entities, rootElement, Core.Version); break;
				case "treasures": Treasures.Load(rootElement, Core.Version); break;
			}
		}
		catch
		{
			// ignored
		}
	}

	public SegmentRegion GetRegion(int id)
	{
		return Regions.FirstOrDefault(region => region.ID == id);
	}
		
	public void UpdateTiles()
	{
		foreach (var region in Regions)
			region.UpdateTiles();
	}
}