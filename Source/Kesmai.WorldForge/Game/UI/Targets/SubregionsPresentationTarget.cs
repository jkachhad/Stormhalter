using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class SubregionsPresentationTarget : WorldPresentationTarget
{
	private SubregionsGraphicsScreen? _screen;
	
	public SegmentSubregion? Subregion { get; set; }
	public SegmentBounds? Bounds { get; set; }
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		_screen = new SubregionsGraphicsScreen(graphicsService, this);
		
		if (Subregion != null)
			_screen.SetSubregion(Subregion);
		
		if (Bounds != null)
			_screen.SetBounds(Bounds);
			
		return (_screen);
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		if (_screen != null)
			_screen.SetSubregion(subregion);
	}

	public void SetBounds(SegmentBounds bounds)
	{
		if (_screen != null)
			_screen.SetBounds(bounds);
	}
}