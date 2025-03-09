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

	public override ItemEntity OnCorpseTanned() => null;
}