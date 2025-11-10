using DigitalRune.Graphics;
using Kesmai.WorldForge.Editor;

namespace Kesmai.WorldForge;

public class LocationsPresentationTarget : WorldPresentationTarget
{
	private LocationsGraphicsScreen? _screen;
	
	public SegmentLocation? Location { get; set; }
		
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		_screen = new LocationsGraphicsScreen(graphicsService, this);

		if (Location != null)
			_screen.SetCameraLocation(Location.X, Location.Y);
		
		return (_screen);
	}

	public void SetCameraLocation(SegmentLocation location)
	{
		if (_screen != null)
			_screen.SetCameraLocation(location.X, location.Y);
	}
}