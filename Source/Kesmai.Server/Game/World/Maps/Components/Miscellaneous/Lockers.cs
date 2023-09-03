using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("LockersComponent")]
public class Lockers : Static, IHandleInteraction
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Lockers"/> class.
	/// </summary>
	public Lockers(XElement element) : base(element)
	{
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Open)
			return false;

		var player = entity as PlayerEntity;
		var location = _parent.Location;

		if (player != null)
		{
			var distance = entity.GetDistanceToMax(location);

			if (distance > 1)
			{
				entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
			}
			else
			{
				entity.LookAt(_parent);
				entity.QueueRoundTimer();
			}
		}

		return true;
	}
}