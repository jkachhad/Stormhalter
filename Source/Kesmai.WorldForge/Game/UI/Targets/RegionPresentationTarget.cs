using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class RegionPresentationTarget : WorldPresentationTarget
{
	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return new WorldGraphicsScreen(graphicsService, this);
	}
}