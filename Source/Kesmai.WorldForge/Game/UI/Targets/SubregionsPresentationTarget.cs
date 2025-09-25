using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class SubregionsPresentationTarget : WorldPresentationTarget
{
	private SubregionsGraphicsScreen _screen;
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new SubregionsGraphicsScreen(graphicsService, this));
	}

	public void SetSubregion(SegmentSubregion subregion)
	{
		if (_screen != null)
			_screen.SetSubregion(subregion);
	}
}