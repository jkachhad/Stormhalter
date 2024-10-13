namespace Kesmai.Server.Game;

public interface IHandleItems
{
	/// <summary>
	/// Called when an item is added.
	/// </summary>
	void OnItemAdded(SegmentTile parent, ItemEntity item, bool isTeleport);

	/// <summary>
	/// Called when an item is removed.
	/// </summary>
	void OnItemRemoved(SegmentTile parent, ItemEntity item, bool isTeleport);
}