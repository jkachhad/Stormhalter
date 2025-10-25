using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushEntry : ObservableObject
{
	private IComponentProvider _component;
	private int _weight = 1;

	public IComponentProvider Component
	{
		get => _component;
		set => SetProperty(ref _component, value);
	}

	public int Weight
	{
		get => _weight;
		set => SetProperty(ref _weight, value);
	}
}
