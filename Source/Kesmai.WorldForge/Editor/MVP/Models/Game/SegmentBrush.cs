using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;

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

	public void Present(ApplicationPresenter presenter)
	{
	}

	public void Copy(Segment segment)
	{
	}
}
