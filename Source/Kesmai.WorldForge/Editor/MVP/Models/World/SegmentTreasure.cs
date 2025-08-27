using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;
using DigitalRune.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor.Scripting;

namespace Kesmai.WorldForge.Editor;

public class SegmentTreasure : ObservableObject, ISegmentObject
{
	private string _name;
        private string _notes;
        public ScriptHost Scripts { get; } = new ScriptHost();
		
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

                Scripts.SetScript("GetChance", string.Empty);
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

                Scripts.SetScript("GetChance", string.Empty);
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

                Scripts.SetScript("GetChance", string.Empty);
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

public class SegmentHoard : SegmentTreasure
{
	private double _chance;
	
	public double Chance
	{
		get => _chance;
		set => SetProperty(ref _chance, value);
	}
	
	public override bool IsHoard => true;
	
	public SegmentHoard()
	{
	}
	
	public SegmentHoard(XElement element) : base(element)
	{
	}
	
	public SegmentHoard(SegmentHoard hoard) : base(hoard)
	{
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		return element;
	}
}

public class TreasureEntry : ObservableObject
{
	public class TreasureEntryWeightChanged : ValueChangedMessage<double>
	{
		public TreasureEntryWeightChanged(double value) : base(value)
		{
		}
	}
	
	private int _weight;
	private string _notes;
	private double _chance;

	private SegmentTreasure _treasure;

	public SegmentTreasure Treasure
	{
		get => _treasure;
		set => SetProperty(ref _treasure, value);
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
	}
		
	public TreasureEntry(SegmentTreasure treasure, XElement element)
	{
		_treasure = treasure;
		_weight = (int)element.Attribute("weight");
			
		if (element.TryGetElement("notes", out var notesElement))
			_notes = (string)notesElement;
	}

	public TreasureEntry(TreasureEntry entry)
	{
		_treasure = entry.Treasure;
		_weight = entry.Weight;

		_notes = entry.Notes;
	}
	
	public XElement GetXElement()
	{
		var element = new XElement("entry",
			new XAttribute("weight", _weight));
			
		if (!String.IsNullOrEmpty(_notes))
			element.Add(new XElement("notes", _notes));

		return element;
	}

	public void InvalidateChance()
	{
		Chance = ((double)_weight / _treasure.TotalWeight);
	}
}