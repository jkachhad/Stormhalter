using DigitalRune.Graphics;

namespace Kesmai.WorldForge;

public class SpawnsPresentationTarget : WorldPresentationTarget
{
	private SpawnsGraphicsScreen _screen;

	public override WorldGraphicsScreen CreateGraphicsScreen(IGraphicsService graphicsService)
	{
		return (_screen = new SpawnsGraphicsScreen(graphicsService, this));
	}

	public void SetLocation(Spawner spawner)
	{
		if (_screen != null)
		{
			_screen.SetSpawner(spawner);
		}
	}
}