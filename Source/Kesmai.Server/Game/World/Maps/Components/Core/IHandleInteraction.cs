namespace Kesmai.Server.Game;

public interface IHandleInteraction
{
	/// <summary>
	/// Handles the interaction from the specified entity.
	/// </summary>
	bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action);
}