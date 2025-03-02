using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class SegmentSubregions : ObservableCollection<SegmentSubregion>
{
	public void Load(XElement element, Version version)
	{
		foreach (var subregionElement in element.Elements("subregion"))
			Add(new SegmentSubregion(subregionElement));
	}

	public void Save(XElement element)
	{
		foreach (var subregion in this)
			element.Add(subregion.GetXElement());
	}
}