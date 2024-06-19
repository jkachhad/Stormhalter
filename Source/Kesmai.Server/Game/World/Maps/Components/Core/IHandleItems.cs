namespace Kesmai.Server.Game;

public interface IHandleItems
{
	/// <summary>
	/// Called when an item is added.
	/// </summary>
	void OnItemAdded(ItemEntity item, bool isTeleport);

	/// <summary>
	/// Called when an item is removed.
	/// </summary>
	void OnItemRemoved(ItemEntity item, bool isTeleport);
}