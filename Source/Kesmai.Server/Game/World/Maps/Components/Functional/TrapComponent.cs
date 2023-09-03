using System;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public abstract class TrapComponent : TerrainComponent, IHandleMovement, IHandlePathing
{
	private DateTime _next;
	
	public TimeSpan Cooldown { get; set; }
	
	public bool TrapCreatures { get; set; }
	public bool TrapPlayers { get; set; }
	
	public bool InterruptMovement { get; set; }

	protected TrapComponent(XElement element) : base(element)
	{
		var cooldownElement = element.Element("cooldown");

		if (cooldownElement != null)
			Cooldown = TimeSpan.FromSeconds((int)cooldownElement);
		else
			Cooldown = TimeSpan.Zero;
		
		var trapCreaturesElement = element.Element("trapCreatures");
		var trapPlayerElement = element.Element("trapPlayer");
		var interruptMovementElement = element.Element("interruptMovement");

		if (trapCreaturesElement != null)
			TrapCreatures = (bool)trapCreaturesElement;
		else
			TrapCreatures = true;

		if (trapPlayerElement != null)
			TrapPlayers = (bool)trapPlayerElement;

		if (interruptMovementElement != null)
			InterruptMovement = (bool)interruptMovementElement;
	}
	
	public void OnEnter(MobileEntity entity)
	{
		/* Execute trap. */
		if ((entity is CreatureEntity && !TrapCreatures) || (entity is PlayerEntity && TrapPlayers))
			return;

		if (Server.Now < _next)
			return;

		_next = Server.Now + Cooldown;
		
		OnSpring(entity);
	}

	/// <summary>
	/// Called when the trap has been triggered by an entity.
	/// </summary>
	protected virtual void OnSpring(MobileEntity entity)
	{
	}
	
	public void OnLeave(MobileEntity entity)
	{
	}

	public int GetMovementCost(MobileEntity entity) => 1;

	public void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (InterruptMovement)
			args.Result = PathingResult.Interrupted;
	}

	public bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return true;
	}

	public bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}
}