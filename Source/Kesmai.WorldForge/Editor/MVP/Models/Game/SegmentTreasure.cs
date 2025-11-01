using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Collections;
using Kesmai.WorldForge.Scripting;
using Kesmai.WorldForge.UI.Documents;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Kesmai.WorldForge.Editor;

public class SegmentTreasureChanged(SegmentTreasure treasure) : ValueChangedMessage<SegmentTreasure>(treasure);

public class SegmentTreasure : ObservableObject, ICloneable, ISegmentObject
{
	protected string _name;
	protected string _notes;
		
	private ObservableCollection<TreasureEntry> _entries = new ObservableCollection<TreasureEntry>();
		
	[Browsable(false)]
	public ObservableCollection<TreasureEntry> Entries
	{
		get => _entries;
		set => SetProperty(ref _entries, value);
	}
		
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentTreasureChanged(this));
		}
	}

	public string Notes
	{
		get => _notes;
		set => SetProperty(ref _notes, value);
	}

	[Browsable(false)]
	public int TotalWeight => _entries.Sum(e => e.Weight);

	[Browsable(false)]
	public virtual bool IsHoard => false;
		
	public SegmentTreasure()
	{
		_entries.Add(new TreasureEntry(this));
		_entries.CollectionChanged += EntriesOnCollectionChanged;

		InvalidateChance();
	}
	#nullable enable
    private void EntriesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
	{
		InvalidateChance();
	}
	#nullable disable
    public SegmentTreasure(XElement element)
	{
		_name = (string)element.Attribute("name");
			
		if (element.TryGetElement("notes", out var notesElement))
			_notes = (string)notesElement;
			
		foreach(var entryElement in element.Elements("entry"))
			_entries.Add(new TreasureEntry(this, entryElement));

		InvalidateChance();
	}

	public SegmentTreasure(SegmentTreasure treasure)
	{
		_name = treasure.Name;
		_notes = treasure.Notes;
		_entries.AddRange(treasure.Entries.Select(e => new TreasureEntry(e)
		{
			Treasure = treasure
		}));

		InvalidateChance();
	}
	
	public virtual void Present(ApplicationPresenter presenter)
	{
		var treasureViewModel = presenter.Documents.OfType<TreasureViewModel>().FirstOrDefault();

		if (treasureViewModel is null)
			presenter.Documents.Add(treasureViewModel = new TreasureViewModel(this));
		else
			treasureViewModel.ActiveTreasure = this;

		if (presenter.ActiveDocument != treasureViewModel)
			presenter.SetActiveDocument(treasureViewModel);
					
		presenter.SetActiveContent(this);
	}
	
	public virtual void Copy(Segment target)
	{
		if (Clone() is SegmentTreasure clonedTreasure)
			target.Treasures.Add(clonedTreasure);
	}

	public virtual XElement GetSerializingElement()
	{
		var element = new XElement("treasure",
			new XAttribute("name", _name));
			
		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));

		foreach (var treasureEntry in _entries)
			element.Add(treasureEntry.GetSerializingElement());

		return element;
	}
	
	public XElement GetReferencingElement()
	{
		return new XElement("treasure",
			new XAttribute("name", _name));
	}
		
	public override string ToString() => _name;

	public void InvalidateChance()
	{
		foreach (var entry in _entries)
			entry.InvalidateChance();
	}
	
	public virtual object Clone()
	{
		return new SegmentTreasure(GetSerializingElement())
		{
			Name = $"Copy of {_name}",
		};
	}
}

[Script("GetChance", "double GetChance(Facet facet, int regionIndex)", "{", "}", "\treturn 100;")]
public class SegmentHoard : SegmentTreasure
{
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
	
	[Browsable(false)]
	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}
	
	public override bool IsHoard => true;
	
	public SegmentHoard()
	{
		ValidateScripts();
	}
	
	public SegmentHoard(XElement element) : base(element)
	{
		ValidateScripts(element);
	}
	
	public SegmentHoard(SegmentHoard hoard) : base(hoard)
	{
		_scripts.Clear();
		_scripts.AddRange(hoard.Scripts.Select(s => s.Clone()));
	}

	public override void Present(ApplicationPresenter presenter)
	{
		var hoardViewModel = presenter.Documents.OfType<HoardViewModel>().FirstOrDefault();

		if (hoardViewModel is null)
			presenter.Documents.Add(hoardViewModel = new HoardViewModel(this));
		else
			hoardViewModel.ActiveHoard = this;

		if (presenter.ActiveDocument != hoardViewModel)
			presenter.SetActiveDocument(hoardViewModel);
					
		presenter.SetActiveContent(this);
	}

	public override void Copy(Segment target)
	{
		if (Clone() is SegmentHoard clonedHoard)
			target.Treasures.Add(clonedHoard);
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
	
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		foreach (var script in _scripts.Where(s => !s.IsEmpty))
			element.Add(script.GetSerializingElement());

		return element;
	}
	
	public override object Clone()
	{
		return new SegmentHoard(GetSerializingElement())
		{
			Name = $"Copy of {_name}",
		};
	}
}
	
[Script("OnCreate", "ItemEntity OnCreate(MobileEntity from, Container container)", "{", "}", "\treturn new ItemEntity();")]
public class TreasureEntry : ObservableObject
{
	public class TreasureEntryWeightChanged : ValueChangedMessage<double>
	{
		public TreasureEntryWeightChanged(double value) : base(value)
		{
		}
	}
		
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
	private int _weight;
	private string _notes;
	private double _chance;

	private SegmentTreasure _treasure;

	public SegmentTreasure Treasure
	{
		get => _treasure;
		set => SetProperty(ref _treasure, value);
	}
		
	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}
		
	public int Weight
	{
		get => _weight;
		set
		{
			SetProperty(ref _weight, value);

			if (value > 0)
				WeakReferenceMessenger.Default.Send(new TreasureEntryWeightChanged(value));
		}
	}

	public string Notes
	{
		get => _notes;
		set => SetProperty(ref _notes, value);
	}

	public double Chance
	{
		get => _chance;
		set => SetProperty(ref _chance, value);
	}

	public TreasureEntry(SegmentTreasure treasure)
	{
		_treasure = treasure;
		_weight = 1;
			
		ValidateScripts();
	}
		
	public TreasureEntry(SegmentTreasure treasure, XElement element)
	{
		_treasure = treasure;
		_weight = (int)element.Attribute("weight");
			
		if (element.TryGetElement("notes", out var notesElement))
			_notes = (string)notesElement;

		ValidateScripts(element);
	}

	public TreasureEntry(TreasureEntry entry)
	{
		_treasure = entry.Treasure;
		_weight = entry.Weight;

		_notes = entry.Notes;

		_scripts.Clear();
		_scripts.AddRange(entry.Scripts.Select(s => s.Clone()));
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

	public XElement GetSerializingElement()
	{
		var element = new XElement("entry",
			new XAttribute("weight", _weight));
			
		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));

		foreach (var script in _scripts.Where(s => !s.IsEmpty))
			element.Add(script.GetSerializingElement());

		return element;
	}

	public void InvalidateChance()
	{
		Chance = ((double)_weight / _treasure.TotalWeight);
	}
}
