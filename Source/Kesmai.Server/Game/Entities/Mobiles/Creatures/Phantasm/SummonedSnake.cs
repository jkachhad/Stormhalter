namespace Kesmai.Server.Game;

public partial class SummonedSnake : Snake
{
	public SummonedSnake() : base()
	{
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
		var focusLevel = 1;
		var magicSkill = player.GetSkillLevel(Skill.Magic);
		var focusItems = player.Inventory.OfType<IPetFocus>().ToList();
		// Search for and get the highest focus level from the items.
		if (focusItems.Count > 0)
			focusLevel += (focusItems.Max(e => e.FocusLevel)*.01);

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