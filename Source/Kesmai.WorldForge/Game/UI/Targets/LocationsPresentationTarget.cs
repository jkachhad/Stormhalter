using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class LocationsPresentationTarget : WorldPresentationTarget
{
	private LocationsGraphicsScreen _screen;
	
	public SegmentLocation Location { get; set; }
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new LocationsGraphicsScreen(graphicsService, this));
	}

	public void SetCameraLocation(SegmentLocation location)
	{
		if (_screen != null)
			_screen.SetCameraLocation(location.X, location.Y);
	}

}