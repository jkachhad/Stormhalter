using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class SummonedPhantom : Phantom
{
	public override bool CanOrderFollow => true;
	public override bool CanOrderAttack => true;
	public override bool CanOrderCarry => true;
	private double _focusLevelModifier = 0.0;

	public SummonedPhantom(double focusLevelModifier = 0.0)
    {
		_focusLevelModifier = focusLevelModifier;
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
        // Focus level is a multiplier for the stats of the pet. 
        double focusLevel = 1;
        var magicSkill = player.GetSkillLevel(Skill.Magic);
        var focusItems = player.Paperdoll.OfType<IPetFocus>().ToList();
		var focusItems2 = player.Rings.OfType<IPetFocus>().ToList();
		var allFocusItems = focusItems.Concat(focusItems2).ToList();
        
		// Search for and get the highest focus level from the items.
		if (allFocusItems.Count > 0)
			focusLevel += (allFocusItems.Max(e => e.FocusLevel) * 0.01);
		
		// Allow for tuning strength without recompiling.
		if (_focusLevelModifier != 0)
			focusLevel += _focusLevelModifier;

        var health = (level + (int)magicSkill)*11 * focusLevel;
        var defense = (level + 9) +((focusLevel - 1)* 10);
		var attack = level +((focusLevel - 1)* 10);
		var magicResist = (level.Clamp(0,40));
        
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
		
		_stats[EntityStat.MagicDamageTakenReduction].Base = magicResist;
		
		Attacks = new CreatureAttackCollection
		{
			{ new CreatureBasicAttack(attack) },
		};
	}
}