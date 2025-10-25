using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class SegmentBrushEntry : ObservableObject
{
	private IComponentProvider _component;
	private int _weight = 1;
	private float _chance = 1.0f;
	
	private SegmentBrush _brush;

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

	public SegmentBrushEntry(SegmentBrush brush)
	{
		_brush = brush;
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);

		if (e.PropertyName != nameof(Weight))
			return;
		
		if (_brush != null)
			_brush.UpdateChances();
	}
}
