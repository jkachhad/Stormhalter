using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

public class SegmentTreasure : ObservableObject, ISegmentObject
{
	private string _name;
	private string _notes;
		
	private ObservableCollection<TreasureEntry> _entries = new ObservableCollection<TreasureEntry>();
		
	public ObservableCollection<TreasureEntry> Entries
	{
		get => _entries;
		set => SetProperty(ref _entries, value);
	}
		
	public string Name
	{
		get => _name;
		set => SetProperty(ref _name, value);
	}
		
	public string Notes
	{
		get => _notes;
		set => SetProperty(ref _notes, value);
	}

	public int TotalWeight => _entries.Sum(e => e.Weight);

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

	public virtual XElement GetXElement()
	{
		var element = new XElement("treasure",
			new XAttribute("name", _name));
			
		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));

		foreach (var treasureEntry in _entries)
			element.Add(treasureEntry.GetXElement());

		return element;
	}
		
	public override string ToString() => _name;

	public void InvalidateChance()
	{
		foreach (var entry in _entries)
			entry.InvalidateChance();
	}
}

[ScriptTemplate("GetChance", typeof(HoardGetChanceScriptTemplate))]
public class SegmentHoard : SegmentTreasure
{
	private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
	private double _chance;
	
	public double Chance
	{
		get => _chance;
		set => SetProperty(ref _chance, value);
	}
	
	public ObservableCollection<Script> Scripts
	{
		get => _scripts;
		set => SetProperty(ref _scripts, value);
	}
	
	public override bool IsHoard => true;
	
	public SegmentHoard() : base()
	{
		ValidateScripts();
	}
	
	public SegmentHoard(XElement element) : base(element)
	{
		foreach (var scriptElement in element.Elements("script"))
			_scripts.Add(new Script(scriptElement));

		ValidateScripts();
	}
	
	public SegmentHoard(SegmentHoard hoard) : base(hoard)
	{
		_scripts.Clear();
		_scripts.AddRange(hoard.Scripts.Select(s => s.Clone()));

		ValidateScripts();
	}
	
	private void ValidateScripts()
	{
		if (_scripts.All(s => s.Name != "GetChance"))
		{
			_scripts.Add(new Script("GetChance", true,
				String.Empty, 
				"\n\treturn 100;\n", 
				String.Empty
			));
		}

		var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
		var attributes = GetType().GetCustomAttributes(typeof(ScriptTemplateAttribute), false)
			.OfType<ScriptTemplateAttribute>().ToList();

		if (attributes.Any())
		{
			foreach (var script in _scripts)
			{
				var attr = attributes.FirstOrDefault(
					a => String.Equals(a.Name, script.Name, StringComparison.Ordinal));

				if (attr != null && provider.TryGetTemplate(attr.TemplateType, out var template))
					script.Template = template;
			}
		}
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		foreach (var script in _scripts)
			element.Add(script.GetXElement());

		return element;
	}
}
	
[ScriptTemplate("OnCreate", typeof(TreasureItemScriptTemplate))]
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

		foreach (var scriptElement in element.Elements("script"))
			_scripts.Add(new Script(scriptElement));
			
		ValidateScripts();
	}

	public TreasureEntry(TreasureEntry entry)
	{
		_treasure = entry.Treasure;
		_weight = entry.Weight;

		_notes = entry.Notes;

		_scripts.Clear();
		_scripts.AddRange(entry.Scripts.Select(s => s.Clone()));
	}
		
	private void ValidateScripts()
	{
		if (_scripts.All(s => s.Name != "OnCreate"))
		{
			_scripts.Add(new Script("OnCreate", true,
				String.Empty, 
				"\n\treturn new ItemEntity();\n", 
				String.Empty
			));
		}

		var provider = ServiceLocator.Current.GetInstance<ScriptTemplateProvider>();
		var attributes = GetType().GetCustomAttributes(typeof(ScriptTemplateAttribute), false)
			.OfType<ScriptTemplateAttribute>().ToList();

		if (attributes.Any())
		{
			foreach (var script in _scripts)
			{
				var attr = attributes.FirstOrDefault(
					a => String.Equals(a.Name, script.Name, StringComparison.Ordinal));

				if (attr != null && provider.TryGetTemplate(attr.TemplateType, out var template))
					script.Template = template;
			}
		}
	}

	public XElement GetXElement()
	{
		var element = new XElement("entry",
			new XAttribute("weight", _weight));
			
		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));

		foreach (var script in _scripts)
			element.Add(script.GetXElement());

		return element;
	}

	public void InvalidateChance()
	{
		Chance = ((double)_weight / _treasure.TotalWeight);
	}
}