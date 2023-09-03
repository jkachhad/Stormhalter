using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("IceComponent")]
public class Ice : Floor, IHandlePathing
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Ice"/> class.
	/// </summary>
	public Ice(XElement element) : base(element)
	{
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return true;
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;

		if (args.Entity is PlayerEntity player)
		{
			var willpower = player.Stats[EntityStat.Willpower].Value;

			if (Utility.Random(1, 20) > willpower)
			{
				args.Entity.SendLocalizedMessage(6100050); /* You slipped on the ice. */
				args.Result = PathingResult.Interrupted;
			}
		}
	}
}