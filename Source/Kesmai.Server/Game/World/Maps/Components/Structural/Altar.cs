using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("AltarComponent")]
public class Altar : TerrainComponent, IHandlePathing, IHandleInteraction
{
	private Terrain _altar;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Altar"/> class.
	/// </summary>
	public Altar(XElement element) : base(element)
	{
		if (element.TryGetElement("altar", out var altarElement))
			_altar = Terrain.Get((int)altarElement, Color);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_altar != null)
			yield return _altar;
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

	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return false;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return false;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (!AllowMovementPath(args.Entity))
			args.Result = PathingResult.Daze;
		else
			args.Result = PathingResult.Allowed;
	}
}