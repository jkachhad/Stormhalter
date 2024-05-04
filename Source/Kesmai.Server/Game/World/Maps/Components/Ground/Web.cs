using System;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("Web")]
public class Web : Static, IHandlePathing
{
	private bool _allowDispel;
		
	private Spell _spell;
	private Timer _dispelTimer;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;
		
	public Web() : this(null, TimeSpan.Zero)
	{
	}
		
	// TODO: Scale for facet time?
	public Web(Spell spell, TimeSpan duration) : base(131)
	{
		_spell = spell;

		if (duration > TimeSpan.Zero)
			_dispelTimer = Timer.DelayCall(duration, Dispel);
			
		_allowDispel = true;
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Web"/> class.
	/// </summary>
	public Web(XElement element) : base(element)
	{
		if (element.TryGetElement("allowDispel", out var allowDispelElement))
			_allowDispel = (bool)allowDispelElement;
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
		args.Result = PathingResult.Allowed;

		if (args.Entity is PlayerEntity player)
		{
			var willpower = player.Stats[EntityStat.Willpower].Value;
			var escapeChance = (player.Level / 3);

			if (willpower > 13)
				escapeChance += (willpower - 13);

			if (Utility.Random(1, 20) > escapeChance)
			{
				args.Entity.SendLocalizedMessage(6100051); /* You are caught in the web. */
				args.Result = PathingResult.Interrupted;
			}
		}
		else if (args.Entity is CreatureEntity creature && !creature.HasImmunity(CreatureImmunity.Web))
		{
			args.Result = PathingResult.Interrupted;
		}
	}

	public void Burn()
	{
		if (_allowDispel)
			Dispel();
	}
		
	public void Dispel()
	{
		if (!_allowDispel)
			return;
			
		if (_dispelTimer != null && _dispelTimer.Running)
			_dispelTimer.Stop();

		_dispelTimer = null;

		if (_parent != null)
			_parent.Remove(this);
	}
}