using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Editor;

public class SegmentTreasures : ObservableCollection<SegmentTreasure>
{
	public string Name => "(Treasures)";

	public void Load(XElement element, Version version)
	{
		foreach (var treasureElement in element.Elements("treasure"))
			Add(new SegmentTreasure(treasureElement));
	}
		
	public void Save(XElement element)
	{
		foreach (var treasure in this)
			element.Add(treasure.GetXElement());
	}
}