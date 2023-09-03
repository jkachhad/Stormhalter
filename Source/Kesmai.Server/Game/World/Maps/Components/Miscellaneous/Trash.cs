using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("TrashComponent")]
public class Trash : Static, IHandleInteraction, IHandleItems
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Trash"/> class.
	/// </summary>
	public Trash(XElement element) : base(element)
	{
	}

	/// <summary>
	/// Called when an item is added.
	/// </summary>
	public void OnItemAdded(ItemEntity item)
	{
		if (item is Corpse)
			return;

		item.Delete();
	}

	/// <summary>
	/// Called when an item is removed.
	/// </summary>
	public void OnItemRemoved(ItemEntity item)
	{
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return false;

		var location = _parent.Location;

		var distance = entity.GetDistanceToMax(location);

		if (distance > 1)
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(_parent);

		return true;
	}
}