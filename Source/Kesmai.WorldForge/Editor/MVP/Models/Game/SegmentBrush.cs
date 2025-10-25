using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
	private readonly ObservableCollection<SegmentBrushEntry> _entries = new ();

	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentBrushChanged(this));
		}
	}

	public ObservableCollection<SegmentBrushEntry> Entries => _entries;

	public int TotalWeight => _entries.Sum(entry => entry.Weight);

	public SegmentBrush()
	{
		_entries ??= new ObservableCollection<SegmentBrushEntry>();
		_entries.CollectionChanged += OnEntriesChanged;
	}

	public SegmentBrush(XElement element)
	{
		_name = (string)element.Attribute("name");
		
		_entries.CollectionChanged += OnEntriesChanged;
	}
	
	public void AddComponent(SegmentTile segmentTile)
	{
		segmentTile.Providers.Add(this);
	}

	public void RemoveComponent(SegmentTile segmentTile)
	{
		segmentTile.Providers.Remove(this);
	}

	public IEnumerable<IComponentProvider> GetComponents()
	{
		yield return this;
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
	}
	
	public void UpdateChances()
	{
		var totalWeight = TotalWeight;

		foreach (var entry in _entries)
			entry.Chance = totalWeight <= 0 ? 0f : (float)entry.Weight / totalWeight;
	}

	public XElement GetXElement()
	{
		return new XElement("brush",
			new XAttribute("name", _name));
	}

	public ComponentFrame GetComponentFrame()
	{
		return new SegmentBrushComponentFrame();
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
		return new SegmentBrush(GetXElement())
		{
			Name = $"Copy of {_name}"
		};
	}
}
