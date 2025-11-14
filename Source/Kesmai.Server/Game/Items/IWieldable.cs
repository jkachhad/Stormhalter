using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public interface IWieldable
{
	void OnWield(MobileEntity entity);

	void OnUnwield(MobileEntity entity);
}