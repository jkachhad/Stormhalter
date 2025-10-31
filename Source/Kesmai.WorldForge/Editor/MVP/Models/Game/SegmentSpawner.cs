using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.Scripting;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge;

public class SegmentSpawnChanged(SegmentSpawner segmentSpawner) : ValueChangedMessage<SegmentSpawner>(segmentSpawner);
	
[Script("OnBeforeSpawn", "void OnBeforeSpawn(Spawner spawner)", "{", "}")]
[Script("OnAfterSpawn", "void OnAfterSpawn(Spawner spawner, MobileEntity spawn)", "{", "}")]
public abstract class SegmentSpawner : ObservableObject, ICloneable, ISegmentObject
{
	private string _name;
	private bool _enabled;

	private TimeSpan _minimumDelay;
	private TimeSpan _maximumDelay;

	private int _maximum;
		
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();

	[Category("Identity")]
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentSpawnChanged(this));
		}
	}

	public abstract void Present(ApplicationPresenter presenter);

	public override string ToString()
	{
		return Name;
	}

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    private string _debug;
    public string Debug
    {
        get => _debug;
        set => SetProperty(ref _debug, value);
    }

    public bool Enabled
	{
		get => _enabled;
		set => SetProperty(ref _enabled, value);
	}
        
	public TimeSpan MinimumDelay
	{
		get => _minimumDelay;
		set => SetProperty(ref _minimumDelay, value);
	}

	public TimeSpan MaximumDelay
	{
		get => _maximumDelay;
		set => SetProperty(ref _maximumDelay, value);
	}

	public int Maximum
	{
		get => _maximum;
		set => SetProperty(ref _maximum, value);
	}

	public ObservableCollection<SpawnEntry> Entries { get; set; } = new ObservableCollection<SpawnEntry>();

	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}
		
	protected SegmentSpawner()
	{
		ValidateScripts();
	}

	protected SegmentSpawner(XElement element)
	{
		_name = (string)element.Attribute("name");
			
		if (Boolean.TryParse((string)element.Attribute("enabled"), out var enabled))
			_enabled = enabled;
		else
			_enabled = true;

		if (Int32.TryParse((string)element.Element("minimumDelay"), out var minSeconds))
			_minimumDelay = TimeSpan.FromSeconds(minSeconds);
		else
			_minimumDelay = TimeSpan.Zero;

		if (Int32.TryParse((string)element.Element("maximumDelay"), out var maxSeconds))
			_maximumDelay = TimeSpan.FromSeconds(maxSeconds);
		else
			_maximumDelay = TimeSpan.Zero;
			
		var maximumElement = element.Element("maximum");

		if (maximumElement != null)
			_maximum = (int)maximumElement;

		ValidateScripts(element);
	}

	private void ValidateScripts(XElement rootElement = default)
	{
		var attributes = GetType().GetCustomAttributes(typeof(ScriptAttribute), inherit: true)
			.Cast<ScriptAttribute>();

		var implementations = new Dictionary<string, Script>();

		if (rootElement != null)
		{
			foreach (var scriptElement in rootElement.Elements("script"))
			{
				var script = new Script(scriptElement);

				if (!implementations.TryAdd(script.Name, script))
					throw new InvalidOperationException($"Duplicate script name '{script.Name}'.");
			}
		}

		foreach (var attribute in attributes)
		{
			if (implementations.TryGetValue(attribute.Name, out var script))
			{
				script.Signature = attribute.Signature;
				script.Header = attribute.Header;
				script.Footer = attribute.Footer;
				
				_scripts.Add(script);
			}
			else
			{
				_scripts.Add(new Script
				{
					Name = attribute.Name,
					Signature = attribute.Signature,
					Header = attribute.Header,
					Body = attribute.Body,
					Footer = attribute.Footer
				});
			}
		}
	}

	public virtual XElement GetSerializingElement()
	{
		var element = new XElement("spawn",
			new XAttribute("type", GetTypeAlias()),
			new XAttribute("name", _name),
			new XAttribute("enabled", _enabled),
			new XElement("minimumDelay", _minimumDelay.TotalSeconds),
			new XElement("maximumDelay", _maximumDelay.TotalSeconds));

		if (_maximum > 0)
			element.Add(new XElement("maximum", _maximum));
			
		foreach (var script in _scripts.Where(s => !s.IsEmpty))
			element.Add(script.GetSerializingElement());

		foreach (var entry in Entries)
		{
			if (entry.SegmentEntity != null)
				element.Add(entry.GetXElement());
		}

		return element;
	}
	
	public virtual XElement GetReferencingElement()
	{
		return new XElement("spawn",
			new XAttribute("name", _name));
	}
		
	protected virtual string GetTypeAlias()
	{
		return GetType().Name;
	}

	public abstract void Copy(Segment segment);
	public abstract object Clone();
}

[Obfuscation(Exclude = true, ApplyToMembers = false)]
public class LocationSegmentSpawner : SegmentSpawner
{
	private int _x;
	private int _y;
	private int _region;
		
	[Category("Coordinates")]
	public int X
	{
		get => _x;
		set => SetProperty(ref _x, value);
	}

	[Category("Coordinates")]
	public int Y
	{
		get => _y;
		set => SetProperty(ref _y, value);
	}
		
	[Category("Coordinates")]
	public int Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
    }

    public LocationSegmentSpawner() : base()
	{
	}

	public LocationSegmentSpawner(XElement element) : base(element)
	{
		var locationElement = element.Element("location");

		if (locationElement != null)
		{
			_x = (int)locationElement.Attribute("x");
			_y = (int)locationElement.Attribute("y");
			_region = (int)locationElement.Attribute("region");
		}
	}
	
	public override void Present(ApplicationPresenter presenter)
	{
		var locationSpawnViewModel = presenter.Documents.OfType<LocationSpawnViewModel>().FirstOrDefault();
		
		if (locationSpawnViewModel is null)
			presenter.Documents.Add(locationSpawnViewModel = new LocationSpawnViewModel());
		
		if (presenter.ActiveDocument != locationSpawnViewModel)
			presenter.SetActiveDocument(locationSpawnViewModel);
		
		presenter.SetActiveContent(this);
	}
	
	public override void Copy(Segment target)
	{
		if (Clone() is LocationSegmentSpawner clonedSpawner)
			target.Spawns.Location.Add(clonedSpawner);
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("location",
			new XAttribute("x", _x),
			new XAttribute("y", _y),
			new XAttribute("region", _region))
		);
			
		return element;
	}

	public override object Clone()
	{
		return new LocationSegmentSpawner(GetSerializingElement())
		{
			Name = $"Copy of {Name}",
		};
	}
}

[Obfuscation(Exclude = true, ApplyToMembers = false)]
public class RegionSegmentSpawner : SegmentSpawner
{
	private int _region;
		
	[Category("Coordinates")]
	public int Region
	{
		get => _region;
		set => SetProperty(ref _region, value);
	}
		
	public ObservableCollection<SegmentBounds> Inclusions { get; set; } = new ObservableCollection<SegmentBounds>();
	public ObservableCollection<SegmentBounds> Exclusions { get; set; } = new ObservableCollection<SegmentBounds>();

	public RegionSegmentSpawner()
	{
		Inclusions.Add(new SegmentBounds());
		Exclusions.Add(new SegmentBounds());
	}

	public RegionSegmentSpawner(XElement element)  : base(element)
	{
		var boundsElement = element.Element("bounds");
			
		if (boundsElement != null)
		{
			var regionAttribute = boundsElement.Attribute("region");
				
			if (regionAttribute != null)
				_region = (int)regionAttribute;
				
			foreach (var rectangleElement in boundsElement.Elements("inclusion"))
			{
				Inclusions.Add(new SegmentBounds(
					(int)rectangleElement.Attribute("left"), 
					(int)rectangleElement.Attribute("top"), 
					(int)rectangleElement.Attribute("right"), 
					(int)rectangleElement.Attribute("bottom")));
			}
				
			foreach (var rectangleElement in boundsElement.Elements("exclusion"))
			{
				Exclusions.Add(new SegmentBounds(
					(int)rectangleElement.Attribute("left"), 
					(int)rectangleElement.Attribute("top"), 
					(int)rectangleElement.Attribute("right"), 
					(int)rectangleElement.Attribute("bottom")));
			}
		}
	}
	
	public override void Present(ApplicationPresenter presenter)
	{
		var regionSpawnViewModel = presenter.Documents.OfType<RegionSpawnViewModel>().FirstOrDefault();

		if (regionSpawnViewModel is null)
			presenter.Documents.Add(regionSpawnViewModel = new RegionSpawnViewModel());

		if (presenter.ActiveDocument != regionSpawnViewModel)
			presenter.SetActiveDocument(regionSpawnViewModel);

		presenter.SetActiveContent(this);
	}
	
	public override void Copy(Segment target)
	{
		if (Clone() is RegionSegmentSpawner clonedSpawner)
			target.Spawns.Region.Add(clonedSpawner);
	}

	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();
		var bounds = new XElement("bounds",
			new XAttribute("region", _region));
			
		foreach (var rectangle in Inclusions.Where(r => r.IsValid))
		{
			bounds.Add(new XElement("inclusion",
				new XAttribute("left", rectangle.Left),
				new XAttribute("top", rectangle.Top),
				new XAttribute("right", rectangle.Right),
				new XAttribute("bottom", rectangle.Bottom)));
		}
			
		foreach (var rectangle in Exclusions.Where(r => r.IsValid))
		{
			bounds.Add(new XElement("exclusion",
				new XAttribute("left", rectangle.Left),
				new XAttribute("top", rectangle.Top),
				new XAttribute("right", rectangle.Right),
				new XAttribute("bottom", rectangle.Bottom)));
		}

		element.Add(bounds);
			
		return element;
	}
	
	public override object Clone()
	{
		return new RegionSegmentSpawner(GetSerializingElement())
		{
			Name = $"Copy of {Name}",
		};
	}
}

public class SpawnEntry : ObservableObject
{
	private SegmentEntity _segmentEntity;
		
	private int _size;
	private int _minimum;
	private int _maximum;
		
	public SegmentEntity SegmentEntity
	{
		get => _segmentEntity;
		set => SetProperty(ref _segmentEntity, value);
	}
		
	public int Size
	{
		get => _size;
		set => SetProperty(ref _size, value);
	}
		
	public int Minimum
	{
		get => _minimum;
		set => SetProperty(ref _minimum, value);
	}
		
	public int Maximum
	{
		get => _maximum;
		set => SetProperty( ref _maximum, value);
	}

	public SpawnEntry()
	{
		Size = 1;
	}

	public SpawnEntry(XElement element)
	{
		Size = (int)element.Attribute("size");
			
		var minimumAttribute = element.Attribute("minimum");
		var maximumAttribute = element.Attribute("maximum");

		if (minimumAttribute != null)
			Minimum = (int)minimumAttribute;
			
		if (maximumAttribute != null)
			Maximum = (int)maximumAttribute;
	}

	public XElement GetXElement()
	{
		return new XElement("entry",
			new XAttribute("entity", _segmentEntity.Name),
			new XAttribute("size", _size),
			new XAttribute("minimum", _minimum),
			new XAttribute("maximum", _maximum));
	}
}