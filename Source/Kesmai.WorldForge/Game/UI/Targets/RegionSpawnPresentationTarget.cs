using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class RegionSpawnPresentationTarget : WorldPresentationTarget
{
	private RegionSpawnGraphicsScreen _screen;
	
	public RegionSegmentSpawner? Spawner { get; set; }

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		_screen = new RegionSpawnGraphicsScreen(graphicsService, this);
		
		if (Spawner != null)
			_screen.SetSpawner(Spawner);
		
		return _screen;
	}

	public void SetSpawner(RegionSegmentSpawner segmentSpawner)
	{
		if (_screen != null)
			_screen.SetSpawner(segmentSpawner);
	}
}
