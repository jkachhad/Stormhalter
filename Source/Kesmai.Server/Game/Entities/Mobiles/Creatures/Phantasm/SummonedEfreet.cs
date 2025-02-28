using System;
using System.IO;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game;

public partial class SummonedEfreet : Efreet
{
	public override bool CanOrderFollow => true;
	public override bool CanOrderAttack => true;
	public override bool CanOrderCarry => true;
		
	public SummonedEfreet(ICreatureSpell spell)
	{
		Summoned = true;
			
		Health = MaxHealth = PowerCurve().health;
		BaseDodge = PowerCurve().defense;
		Mana = MaxMana = 40;
		Movement = 3;

		Attacks = new CreatureAttackCollection
		{
			{ new CreatureBasicAttack(PowerCurve().attack) },
		};

		Blocks = new CreatureBlockCollection
		{
			new CreatureBlock(17, "a hand"),
		};

		Spells = new CreatureSpellCollection()
		{
			{ new CreatureSpellEntry(spell, 100, TimeSpan.FromSeconds(3) )}
		};
			
		AddStatus(new NightVisionStatus(this));
		AddStatus(new PoisonProtectionStatus(this));
			
		CanFly = true;
	}
	private (int health, int defense, int attack, int magicResist) PowerCurve()
    {
        var player = Director;
        var level = player.Level;
        var magicSkill = player.GetSkillLevel(Skill.Magic);

        var health = (level + (int)magicSkill)*11;
        var defense = (30 + ((level - 21)* 0.5));
		var attack = level - 3;
		var magicResist = (level + 9).Clamp(30,40);
        
        return (health,(int)defense, attack, magicResist);
    }		
	protected override void OnCreate()
	{
		base.OnCreate();
		
		_stats[EntityStat.FireProtection].Base = 100;
		_stats[EntityStat.MagicDamageTakenReduction].Base = PowerCurve().magicResist;
	}
		
	protected override void OnLoad()
	{
		base.OnLoad();
			
		_brain = new CombatAI(this);
	}

	public override bool AllowDamageFrom(Spell spell)
	{
		if (Spells.Any((e => e.Spell.SpellType == spell.GetType()), out CreatureSpellEntry entry))
			return false;

		return true;
	}
	public override void OnSpellTarget(Target target, MobileEntity combatant)
	{
		if (Spell is DragonBreathSpell dragonBreath)
		{
			var direction = GetDirectionTo(combatant.Location);
			var distance = GetDistanceToMax(combatant.Location);

			if (direction == Direction.None)
				direction = Direction.Cardinal.Random();

			if (distance >= 3)
				dragonBreath.CastAt(direction);
			else
				dragonBreath.CastAt(direction, direction.Opposite);

			if (target != null)
				target.Cancel(this, TargetCancel.Canceled);
		}
		else
		{
			base.OnSpellTarget(target, combatant);
		}

	}
}