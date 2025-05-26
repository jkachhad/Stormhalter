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
	
	private (int health, int defense) GetSnakeStats()
	{
		var player = Director;
        var level = player.Level;
        // Focus level is a multiplier for the stats of the pet. 
        double focusLevel = 1;
        var magicSkill = player.GetSkillLevel(Skill.Magic);
        var focusItemsWorn = player.Paperdoll.OfType<IPetFocus>().ToList();
        var focusItemsHands = player.Hands.OfType<IPetFocus>().ToList();
        var focusItems = focusItemsWorn.Concat(focusItemsHands).ToList();
        
		// Search for and get the highest focus level from the items.
		if (focusItems.Count > 0)
			focusLevel += (focusItems.Max(e => e.FocusLevel) * 0.01);
		
		// Allow for tuning strength without recompiling.
		if (_focusLevelModifier != 0)
			focusLevel += _focusLevelModifier;

		var health = (level + (int)magicSkill)* 5 * focusLevel;
		var defense = (level) * focusLevel;

		return ((int)health,(int)defense);
	}
	
	public override void OnEnterWorld()
	{
		base.OnEnterWorld();
		
		var (health, defense) = GetSnakeStats();
		
		Health = MaxHealth = health;
		BaseDodge = defense;
	}

	public override ItemEntity OnCorpseTanned() => null;
}