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
			
		Health = MaxHealth = 1;
		BaseDodge = 1;
		Mana = MaxMana = 40;
		Movement = 3;
		
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
	    // Focus level is a multiplier for the stats of the pet. 
	    var focusLevel = 1;
	    var magicSkill = player.GetSkillLevel(Skill.Magic);
	    var focusItems = player.Inventory.OfType<IPetFocus>().ToList();
	    // Search for and get the highest focus level from the items.
	    if (focusItems.Count > 0)
		    focusLevel += (focusItems.Max(e => e.FocusLevel)*.01);

        var health = (level + (int)magicSkill)*11* focusLevel;
        var defense = (30 + ((level - 21)* 0.5)) * focusLevel;
		var attack = (level - 3) * focusLevel;
		var magicResist = ((level + 9).Clamp(30,40))* focusLevel;
        
        return ((int)health,(int)defense, (int)attack, (int)magicResist);
    }		

	protected override void OnLoad()
	{
		base.OnLoad();
			
		_brain = new CombatAI(this);
	}
	
	public override void OnEnterWorld()
	{
		base.OnEnterWorld();
		
		var (health, defense, attack, magicResist) = PowerCurve();
		
		Health = MaxHealth = health;
		BaseDodge = defense;
		
		_stats[EntityStat.FireProtection].Base = 100;
		_stats[EntityStat.MagicDamageTakenReduction].Base = magicResist;
		
		Attacks = new CreatureAttackCollection
		{
			{ new CreatureBasicAttack(attack) },
		};
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