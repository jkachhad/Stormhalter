using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public partial interface IWieldable
{
	void OnWield(MobileEntity entity);

	void OnUnwield(MobileEntity entity);
}