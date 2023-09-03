namespace Kesmai.Server.Game;

public interface IHandleInteraction
{
	/// <summary>
	/// Handles the interaction from the specified entity.
	/// </summary>
	bool HandleInteraction(MobileEntity entity, ActionType action);
}