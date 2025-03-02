using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class SegmentLocations : ObservableCollection<SegmentLocation>
{
	public string Name => "(Locations)";

	public void Load(XElement element, Version version)
	{
		foreach (var locationElement in element.Elements("location"))
			Add(new SegmentLocation(locationElement));
	}

	public void Save(XElement element)
	{
		foreach (var location in this)
			element.Add(location.GetXElement());
	}
}