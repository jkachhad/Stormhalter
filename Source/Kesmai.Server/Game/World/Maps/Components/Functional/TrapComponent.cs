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

public abstract class SpellTrapComponent<T> : TrapComponent where T : Spell, new()
{
	public int Skill { get; set; }

	protected SpellTrapComponent(XElement element) : base(element)
	{
		var skillElement = element.Element("skill");

		if (skillElement != null)
			Skill = (int)skillElement;
	}
	
	public abstract void CastSpell(T spell, MobileEntity entity);

	protected override void OnSpring(MobileEntity entity)
	{
		base.OnSpring(entity);

		if (!entity.IsAlive)
			return;

		var spell = new T()
		{
			Cost = 0,
			SkillLevel = Skill,
		};
		
		CastSpell(spell, entity);
	}
}

[WorldForgeComponent("WhirlwindTrap")]
public class WhirlwindTrap : SpellTrapComponent<WhirlwindSpell>
{
	public Direction Direction { get; set; }
	
	public WhirlwindTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("direction", out var directionElement))
			Direction = Direction.GetDirection((int)directionElement);
	}
	
	public override void CastSpell(WhirlwindSpell spell, MobileEntity entity)
	{
		var damage = 12 * Skill;
		var strength = (4 * Skill) - 40;
		
		var whirlwind = new Whirlwind(spell, (int)damage, (int)strength, Direction,
			(Direction != Direction.East && Direction != Direction.West));
		
		Parent.Add(whirlwind);
	}
}

[WorldForgeComponent("PoisonCloudTrap")]
public class PoisonCloudTrap : SpellTrapComponent<PoisonCloudSpell>
{
	public Direction Direction { get; set; }
	
	public PoisonCloudTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("direction", out var directionElement))
			Direction = Direction.GetDirection((int)directionElement);
	}
	
	public override void CastSpell(PoisonCloudSpell spell, MobileEntity entity)
	{
		var poisonCloud = new PoisonCloud(spell, Skill, 0, Direction,
			(Direction != Direction.East && Direction != Direction.West));
		
		Parent.Add(poisonCloud);
	}
}

[WorldForgeComponent("BonfireTrap")]
public class BonfireTrap : SpellTrapComponent<BonfireSpell>
{
	public BonfireTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(BonfireSpell spell, MobileEntity entity)
	{
		Parent.Add(new Fire(spell, 5 * Skill, 
			entity.Facet.TimeSpan.FromRounds(Skill + 4)));
	}
}

[WorldForgeComponent("FirestormTrap")]
public class FirestormTrap : SpellTrapComponent<FireStormSpell>
{
	public FirestormTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(FireStormSpell spell, MobileEntity entity)
	{
		Parent.Add(new FireStorm(spell, 10 * Skill, Skill));
	}
}

[WorldForgeComponent("ConcussionTrap")]
public class ConcussionTrap : SpellTrapComponent<ConcussionSpell>
{
	public ConcussionTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(ConcussionSpell spell, MobileEntity entity)
	{
		Parent.Add(new Explosion(spell, 8 * Skill, 0));
	}
}

[WorldForgeComponent("LightningBoltTrap")]
public class LightningBoltTrap : SpellTrapComponent<LightningBoltSpell>
{
	public LightningBoltTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(LightningBoltSpell spell, MobileEntity entity)
	{
		Parent.Add(new Lightning(spell, Skill, 6 * Skill));
	}
}

[WorldForgeComponent("BlindTrap")]
public class BlindTrap : SpellTrapComponent<BlindSpell>
{
	public BlindTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(BlindSpell spell, MobileEntity entity)
	{
		if (BlindSpell.IsBlinded(entity, Skill, out var rounds))
		{
			foreach (var defender in entity.Group.Members)
				defender.Fear(rounds + 3);
		}
	}
}

[WorldForgeComponent("FearTrap")]
public class FearTrap : SpellTrapComponent<FearSpell>
{
	public FearTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(FearSpell spell, MobileEntity entity)
	{
		if (FearSpell.IsFeared(entity, Skill, out var rounds))
		{
			foreach (var defender in entity.Group.Members)
				defender.Fear(rounds + 5);
		}
	}
}

[WorldForgeComponent("StunTrap")]
public class StunTrap : SpellTrapComponent<StunSpell>
{
	public StunTrap(XElement element) : base(element)
	{
	}
	
	public override void CastSpell(StunSpell spell, MobileEntity entity)
	{
		if (StunSpell.IsStunned(entity, Skill, out var rounds))
		{
			var maxRounds = (entity is CreatureEntity) ? 5 : 3;
			
			if (rounds > maxRounds)
				rounds = maxRounds;
			
			foreach (var defender in entity.Group.Members)
				defender.Fear((rounds * 3) + 3);
		}
	}
}