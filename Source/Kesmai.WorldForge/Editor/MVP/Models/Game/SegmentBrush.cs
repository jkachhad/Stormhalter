using System;
using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushChanged(SegmentBrush brush) : ValueChangedMessage<SegmentBrush>(brush);

public class SegmentBrush : ObservableObject, ISegmentObject
{
	private string _name;

	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentBrushChanged(this));
		}
	}

	public SegmentBrush()
	{
	}
	
	public SegmentBrush(XElement element)
	{
		_name = (string)element.Attribute("name");
	}

	public XElement GetXElement()
	{
		return new XElement("brush",
			new XAttribute("name", _name));
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
