using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class LocationSpawnPresentationTarget : WorldPresentationTarget
{
	private LocationSpawnGraphicsScreen _screen;
	
	public LocationSegmentSpawner? Spawner { get; set; }

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		_screen = new LocationSpawnGraphicsScreen(graphicsService, this);
		
		if (Spawner != null)
			_screen.SetSpawner(Spawner);

		return _screen;
	}

	public void SetSpawner(LocationSegmentSpawner segmentSpawner)
	{
		if (_screen != null)
			_screen.SetSpawner(segmentSpawner);
	}
}
