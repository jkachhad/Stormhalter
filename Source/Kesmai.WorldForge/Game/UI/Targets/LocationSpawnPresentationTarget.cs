using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class LocationSpawnPresentationTarget : WorldPresentationTarget
{
	private LocationSpawnGraphicsScreen _screen;

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return _screen = new LocationSpawnGraphicsScreen(graphicsService, this);
	}

	public void SetSpawner(LocationSpawner spawner)
	{
		if (_screen != null)
			_screen.SetSpawner(spawner);
	}
}
