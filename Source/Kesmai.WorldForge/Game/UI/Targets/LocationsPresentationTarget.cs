using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class LocationsPresentationTarget : WorldPresentationTarget
{
	private LocationsGraphicsScreen _screen;
		
	public override bool AllowInput => true;
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new LocationsGraphicsScreen(graphicsService, this));
	}

	public void SetLocation(SegmentLocation location)
	{
		if (_screen != null)
			_screen.SetLocation(location.X, location.Y);
	}

}