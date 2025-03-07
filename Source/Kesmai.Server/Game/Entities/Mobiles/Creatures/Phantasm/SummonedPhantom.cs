using System;
using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class SummonedPhantom : Phantom
{
	public override bool CanOrderFollow => true;
	public override bool CanOrderAttack => true;
	public override bool CanOrderCarry => true;

	public SummonedPhantom()
    {
        Summoned = true;
            
        Health = MaxHealth = 1;
        BaseDodge = 1;
        Movement = 3;
        
        Blocks = new CreatureBlockCollection
        {
            new CreatureBlock(12, "a hand"),
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
        var defense = level + 9;
		var attack = level;
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
		
		_stats[EntityStat.MagicDamageTakenReduction].Base = magicResist;
		
		Attacks = new CreatureAttackCollection
		{
			{ new CreatureBasicAttack(attack) },
		};
	}
}