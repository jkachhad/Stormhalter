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

namespace Kesmai.WorldForge.Editor;

public interface ISegmentObject
{
	string Name { get; set; }
}

public class Segment : ObservableObject
{
	private static List<string> _reservedLocations = new List<string>()
	{
		"Entrance", "Resurrect", "Facet", "Thief", 
	};
		
	private string _name;
	private string _path;

	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}

	public string Path
	{
		get => _path;
		set => _path = value;
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