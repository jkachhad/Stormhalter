using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using Ionic.Zip;

namespace Kesmai.WorldForge.Editor;

public class SegmentLocations : ObservableCollection<SegmentLocation>
{
	public string Name => "(Locations)";
		
	public void Load(ZipFile archive, Version version)
	{
		var archiveEntry = archive["locations"];

		if (archiveEntry != null)
		{
			var locationsElement = XDocument.Load(archiveEntry.OpenReader()).Root;

			if (locationsElement != null)
				Load(locationsElement, version);
		}
	}

	public void Load(XElement element, Version version)
	{
		foreach (var locationElement in element.Elements("location"))
			Add(new SegmentLocation(locationElement));
	}

#if (ArchiveStorage)
		public void Save(ZipFile archive)
		{
			var locationsDocument = new XDocument();
			var locationsElement = new XElement("locations");

			foreach (var location in this)
				locationsElement.Add(location.GetXElement());
			
			locationsDocument.Add(locationsElement);
			
			var memoryStream = new MemoryStream();
			var streamWriter = new StreamWriter(memoryStream);

			locationsDocument.Save(streamWriter);

			memoryStream.Position = 0;

			archive.AddEntry(@"locations", memoryStream);
		}
#else
	public void Save(XElement element)
	{
		foreach (var location in this)
			element.Add(location.GetXElement());
	}
#endif	
}