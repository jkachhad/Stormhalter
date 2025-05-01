namespace Kesmai.Server.Items;

public interface ICharged
{
	int ChargesCurrent { get; set; }
		
	int ChargesMax { get; set; }
}