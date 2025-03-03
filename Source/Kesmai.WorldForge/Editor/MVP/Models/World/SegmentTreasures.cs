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
		{
			var isHoardAttribute = treasureElement.Attribute("hoard");

			if (isHoardAttribute != null && (bool)isHoardAttribute)
				Add(new SegmentHoard(treasureElement));
			else
				Add(new SegmentTreasure(treasureElement));
		}
	}
		
	public void Save(XElement element)
	{
		foreach (var treasure in this)
		{
			var treasureElement = treasure.GetXElement();

			if (treasure is SegmentHoard)
				treasureElement.Add(new XAttribute("hoard", true));
			
			element.Add(treasureElement);
		}
	}
}