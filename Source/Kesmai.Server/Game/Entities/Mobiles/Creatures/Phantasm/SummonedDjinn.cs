using System;
using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class SummonedDjinn : Djinn
{
	public override bool CanOrderFollow => true;
	public override bool CanOrderAttack => true;
	public override bool CanOrderCarry => true;

	public SummonedDjinn(ICreatureSpell spell)
	{
		Summoned = true;
			
		Health = MaxHealth = 1;
		BaseDodge = 1;
		Mana = MaxMana = 24;

		Movement = 3;
		
		Blocks = new CreatureBlockCollection
		{
			new CreatureBlock(12, "a hand"),
		};
			
		Spells = new CreatureSpellCollection()
		{
			{ new CreatureSpellEntry(spell, 100, TimeSpan.FromSeconds(3) ) }	
		};
			
		AddStatus(new NightVisionStatus(this));

		CanFly = true;
	}
	private (int health, int defense, int attack, int magicResist) PowerCurve()
    {
        var player = Director;
        var level = player.Level;
        var magicSkill = player.GetSkillLevel(Skill.Magic);

        var health = (level + (int)magicSkill)*11;
        var defense = level + 3;
		var attack = level - 3;
		var magicResist = level.Clamp(0,40);
        
        return (health, defense, attack, magicResist);
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
		
		_stats[EntityStat.IceProtection].Base = 90;
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
}