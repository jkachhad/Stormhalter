using CommunityToolkit.Mvvm.ComponentModel;
using Kesmai.WorldForge;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushEntry : ObservableObject
{
	private IComponentProvider _component;
	private int _weight = 1;
	private float _chance = 1.0f;

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
	
	public float Chance
	{
		get => _chance;
		set => SetProperty(ref _chance, value);
	}
}
