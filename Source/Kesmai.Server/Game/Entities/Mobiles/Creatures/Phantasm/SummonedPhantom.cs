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
            
        Health = MaxHealth = PowerCurve().health;
        BaseDodge = PowerCurve().defense;
        Movement = 3;

        Attacks = new CreatureAttackCollection
        {
            { new CreatureBasicAttack(14) },
        };

        Blocks = new CreatureBlockCollection
        {
            new CreatureBlock(12, "a hand"),
        };
            
        AddStatus(new NightVisionStatus(this));
            
        CanFly = true;
    }

    private (int health, int defense) PowerCurve()
    {
        var player = Director;
        var level = player.Level;
        var magicSkill = player.GetSkillLevel(Skill.Magic);

        var health = (level + (int)magicSkill)*11;
        var defense = (level + 10).Clamp(0,25);
        
        return (health,defense);
    }	
		
	protected override void OnCreate()
	{
		base.OnCreate();

		_stats[EntityStat.MagicDamageTakenReduction].Base = 10;
	}
		
	protected override void OnLoad()
	{
		base.OnLoad();
			
		_brain = new CombatAI(this);
	}
}