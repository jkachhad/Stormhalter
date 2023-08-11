using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using Ionic.Zip;

namespace Kesmai.WorldForge.Editor;

public class SegmentSubregions : ObservableCollection<SegmentSubregion>
{
		
		
	public void Load(ZipFile archive, Version version)
	{
		var archiveEntry = archive["subregions"];

		if (archiveEntry != null)
		{
			var subregionsElement = XDocument.Load(archiveEntry.OpenReader()).Root;

			if (subregionsElement != null)
				Load(subregionsElement, version);
		}
	}
		
	public void Load(XElement element, Version version)
	{
		foreach (var subregionElement in element.Elements("subregion"))
			Add(new SegmentSubregion(subregionElement));
	}

#if (ArchiveStorage)
		public void Save(ZipFile archive)
		{
			var subregionDocument = new XDocument();
			var subregionsElement = new XElement("subregions");

			foreach (var subregion in this)
				subregionsElement.Add(subregion.GetXElement());
			
			subregionDocument.Add(subregionsElement);
			
			var memoryStream = new MemoryStream();
			var streamWriter = new StreamWriter(memoryStream);

			subregionDocument.Save(streamWriter);

			memoryStream.Position = 0;

			archive.AddEntry(@"subregions", memoryStream);
		}
#else
	public void Save(XElement element)
	{
		foreach (var subregion in this)
			element.Add(subregion.GetXElement());
	}
#endif
}