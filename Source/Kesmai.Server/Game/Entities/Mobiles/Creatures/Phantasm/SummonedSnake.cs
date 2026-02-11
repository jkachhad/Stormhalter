using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class SummonedSnake : Snake
{
	private double _focusLevelModifier = 0.0;

	public SummonedSnake(double focusLevelModifier = 0.0) : base()
	{
		_focusLevelModifier = focusLevelModifier;
		Alignment = Alignment.Lawful;
		
		Health = MaxHealth = 40;
		BaseDodge = 14;
		
		Summoned = true;
	}
	
	private (int health, int defense, double poisonStrength, int attackLevel) GetSnakeStats()
	{
		var player = Director;
        attackLevel = Math.Max(8,player.Level - 10);
        // Focus level is a multiplier for the stats of the pet. 
        double focusLevel = 1;
        var magicSkill = player.GetSkillLevel(Skill.Magic);
        var focusItems = player.Paperdoll.OfType<IPetFocus>().ToList();
		var focusItems2 = player.Rings.OfType<IPetFocus>().ToList();
		var allFocusItems = focusItems.Concat(focusItems2).ToList();
		poisonStrength = Math.Max(1, magicSkill - 10);
        
		// Search for and get the highest focus level from the items.
		if (allFocusItems.Count > 0)
			focusLevel += (allFocusItems.Max(e => e.FocusLevel) * 0.01);
		
		// Allow for tuning strength without recompiling.
		if (_focusLevelModifier != 0)
			focusLevel += _focusLevelModifier;

		var health = (player.level + (int)magicSkill)* 5 * focusLevel;
		var defense = player.level * focusLevel;
		poisonStrength *= focusLevel;

		return ((int)health,(int)defense,poisonStrength,attackLevel);
	}
	
	public override void OnEnterWorld()
	{
		base.OnEnterWorld();
		
		var (health, defense, poisonStrength, attackLevel) = GetSnakeStats();

		Attacks = new CreatureAttackCollection
		{
			{ 
            	new CreatureAttack(attackLevel, 5, 15, "The snake strikes you."),    60 
			},
			{ 
				new CreatureAttack(attackLevel, 5, 15, "The snake strikes you with its fangs, injecting venom.", new AttackPoisonComponent((int)poisonStrength)),      40
			},
		};
		
		Health = MaxHealth = health;
		BaseDodge = defense;
	}

	public override ItemEntity OnCorpseTanned() => null;
}