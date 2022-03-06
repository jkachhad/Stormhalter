namespace Kesmai.Server.Items
{
    public partial interface IFragile
    {
        int DurabilityCurrent { get; set; }
		
        int DurabilityMax { get; set; }
    }
}
