using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class RegionSpawnPresentationTarget : WorldPresentationTarget
{
	private RegionSpawnGraphicsScreen _screen;

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return _screen = new RegionSpawnGraphicsScreen(graphicsService, this);
	}

	public void SetSpawner(RegionSegmentSpawner segmentSpawner)
	{
		if (_screen != null)
			_screen.SetSpawner(segmentSpawner);
	}
}
