using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Xna.Framework;

namespace Kesmai.WorldForge.Editor;

public class SegmentBounds : ObservableObject
{
	private int _left;
	private int _top;
	private int _right;
	private int _bottom;

	public int Left
	{
		get => _left;
		set => SetProperty(ref _left, value);
	}
		
	public int Top
	{
		get => _top;
		set => SetProperty(ref _top, value);
	}
		
	public int Right
	{
		get => _right;
		set => SetProperty(ref _right, value);
	}
		
	public int Bottom
	{
		get => _bottom;
		set => SetProperty(ref _bottom, value);
	}

	public int Width => (_right - _left) + 1;
	public int Height => (_bottom - _top) + 1;

	public bool IsValid => (Width != 0 && Height != 0);

	public SegmentBounds()
	{
	}

	public SegmentBounds(int left, int top, int right, int bottom)
	{
		Left = left;
		Top = top;
		Right = right;
		Bottom = bottom;
	}

	public Rectangle ToRectangle()
	{
		return new Rectangle(Left, Top, Width, Height);
	}
}