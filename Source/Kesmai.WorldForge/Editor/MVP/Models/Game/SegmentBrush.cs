using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;
using Kesmai.WorldForge.Windows;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushChanged(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);

public class SegmentBrush : ObservableObject, ISegmentObject, IComponentProvider
{
	private string _name;
	private readonly ObservableCollection<SegmentBrushEntry> _entries;

	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentBrushChanged(this));
		}
	}

	[Browsable(false)]
	public ObservableCollection<SegmentBrushEntry> Entries => _entries;

	[Browsable(false)]
	public int TotalWeight => _entries.Sum(entry => entry.Weight);

	public SegmentBrush()
	{
		_entries = new ObservableCollection<SegmentBrushEntry>();
		_entries.CollectionChanged += OnEntriesChanged;
	}

	public SegmentBrush(XElement element) : this()
	{
		_name = (string)element.Attribute("name");

		foreach (var entryElement in element.Elements())
			_entries.Add(new SegmentBrushEntry(this, entryElement));
	}
	
	public void AddComponent(ObservableCollection<IComponentProvider> collection)
	{
		collection.Add(this);
	}

	public void RemoveComponent(ObservableCollection<IComponentProvider> collection)
	{
		collection.Remove(this);
	}

	public IEnumerable<IComponentProvider> GetComponents()
	{
		yield return this;
	}

	public IEnumerable<ComponentRender> GetRenders(int mx, int my)
	{
		var entries = _entries.Where(entry => entry.Component is not null && entry.Weight > 0).ToArray();

		if (entries.Length is 0)
			yield break;

		var totalWeight = entries.Sum(entry => entry.Weight);

		if (totalWeight <= 0)
			yield break;

		var entry = SelectEntry(entries, totalWeight, mx, my);

		if (entry?.Component is null)
			yield break;

		foreach (var render in entry.Component.GetRenders(mx, my))
			yield return render;
	}
	
	public IEnumerable<ComponentRender> GetRenders()
	{
		if (_entries.Any())
		{
			foreach (var render in _entries.First().Component.GetRenders())
				yield return render;
		}
	}

	private void OnEntriesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		UpdateChances();
		
		WeakReferenceMessenger.Default.Send(new SegmentBrushChanged(this));
	}
	
	public void UpdateChances()
	{
		var totalWeight = TotalWeight;

		foreach (var entry in _entries)
			entry.Chance = totalWeight <= 0 ? 0f : (float)entry.Weight / totalWeight;
	}

	public XElement GetSerializingElement()
	{
		var element = new XElement("brush",
			new XAttribute("name", _name));
		
		foreach (var entry in _entries)
			element.Add(entry.GetSerializingElement());

		return element;
	}
	
	public XElement GetReferencingElement()
	{
		return new XElement("brush", new XAttribute("name", _name));
	}

	public ComponentFrame GetComponentFrame()
	{
		return new SegmentBrushComponentFrame(this);
	}

	public void Present(ApplicationPresenter presenter)
	{
		var viewModel = presenter.Documents.OfType<SegmentBrushViewModel>().FirstOrDefault();

		if (viewModel is null)
			presenter.Documents.Add(viewModel = new SegmentBrushViewModel());

		if (presenter.ActiveDocument != viewModel)
			presenter.SetActiveDocument(viewModel);

		presenter.SetActiveContent(this);
	}

	public void Copy(Segment target)
	{
		if (Clone() is SegmentBrush segmentBrush)
			target.Brushes.Add(segmentBrush);
	}
	
	public object Clone()
	{
		return new SegmentBrush(GetSerializingElement())
		{
			Name = $"Copy of {_name}"
		};
	}

	private SegmentBrushEntry SelectEntry(IReadOnlyList<SegmentBrushEntry> entries, int totalWeight, int mx, int my)
	{
		if (entries.Count is 0 || totalWeight <= 0)
			return null;

		var seed = HashCode.Combine(mx, my, totalWeight, entries.Count, Name);
		var random = new Random(seed);
		var roll = random.Next(totalWeight);
		var cumulative = 0;

		foreach (var entry in entries)
		{
			cumulative += entry.Weight;

			if (roll < cumulative)
				return entry;
		}

		return entries[^1];
	}
}
