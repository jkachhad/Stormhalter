using System.Linq;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Kesmai.WorldForge.Editor;
using Kesmai.WorldForge.UI.Documents;

namespace Kesmai.WorldForge;

public class SegmentComponentChanged(SegmentComponent segmentComponent) : ValueChangedMessage<SegmentComponent>(segmentComponent);

public class SegmentComponent : ObservableObject, ISegmentObject
{
	private string _name;
	private XElement _element;
	
	public string Name
	{
		get => _name;
		set
		{
			if (SetProperty(ref _name, value))
				WeakReferenceMessenger.Default.Send(new SegmentComponentChanged(this));
		}
	}
	
	public XElement Element
	{
		get => _element;
		set 
		{
			if (SetProperty(ref _element, value))
				WeakReferenceMessenger.Default.Send(new SegmentComponentChanged(this));
		}
	}
	
	public object Clone()
	{
		var clone = new SegmentComponent()
		{
			Name = $"Copy of {_name}",
			Element = _element,
		};
		
		return clone;
	}
	
	public void Present(ApplicationPresenter presenter)
	{
		var componentViewModel = presenter.Documents.OfType<ComponentViewModel>().FirstOrDefault();

		if (componentViewModel is null)
			presenter.Documents.Add(componentViewModel = new ComponentViewModel());

		if (presenter.ActiveDocument != componentViewModel)
			presenter.SetActiveDocument(componentViewModel);

		presenter.SetActiveContent(this);
	}

	public void Copy(Segment target)
	{
		if (Clone() is SegmentComponent segmentComponent)
			target.Components.Add(segmentComponent);
	}
}
