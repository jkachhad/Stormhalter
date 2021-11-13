namespace Kesmai.Server.Items
{
	public partial interface ICharged
	{
		int ChargesCurrent { get; set; }
		
		int ChargesMax { get; set; }
	}
}