using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("WaterComponent")]
public class Water : Floor, IHandlePathing
{
	private int _depth;
		
	/// <summary>
	/// Gets the depth.
	/// </summary>
	public int Depth => _depth;

	/// <summary>
	/// Gets a value indicating if an entity can be drowned.
	/// </summary>
	public bool CanDrown => (_depth >= 3);
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Water"/> class.
	/// </summary>
	public Water(XElement element) : base(element)
	{
		if (element.TryGetElement("depth", out var depthElement))
			_depth = (int)depthElement;
		else
			_depth = 3;
	}
		
	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
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
		var entity = args.Entity;

		if ((entity is CreatureEntity creature && creature.CanFly) || entity.HasStatus(typeof(WaterWalkingStatus)))
			args.Result = PathingResult.Allowed;
		else
			args.Result = PathingResult.Interrupted;
	}

	/// <inheritdoc />
	/// <remarks>
	/// Water that has depth less than 3 will have reduced costs. Otherwise,
	/// the cost is always 3, unless the player can water walk.
	/// </remarks>
	public override int GetMovementCost(MobileEntity entity)
	{
		var cost = base.GetMovementCost(entity);

		if (entity.HasStatus(typeof(WaterWalkingStatus)))
			return 1;
			
		return Math.Min(_depth, cost);
	}

	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public override void OnEnter(MobileEntity entity)
	{
		if (!entity.IsAlive)
			return;
			
		/* Check for any open bottles in their hands. */
		var hands = entity.Hands;

		if (hands != null)
		{
			var bottles = hands.OfType<Bottle>().Where(b => b.IsOpen);

			foreach (var bottle in bottles)
			{
				bottle.Content = new ConsumableWater();
				bottle.Owner = entity;
			}
		}

		var silent = (entity is CreatureEntity creature && (creature.CanSwim || creature.CanFly));	

		if (!silent)
			_parent.PlaySound(59, 3, 6);
			
		var drowning = true;

		if (entity is CreatureEntity swimmer)
			drowning = !swimmer.CanFly;
		
		if (entity.HasStatus(typeof(BreatheWaterStatus)))
			return;

		var region = _parent.Region;

		if (CanDrown && drowning && !region.IsInactive)
			entity.QueueWaterTimer(_depth);
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public override void OnLeave(MobileEntity entity)
	{
		entity.StopWaterTimer();
	}
}