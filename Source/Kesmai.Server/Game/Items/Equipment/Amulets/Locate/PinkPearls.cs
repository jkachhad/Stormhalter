using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PinkPearls : LocateAmulet
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 200;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;
		
		public PinkPearls() : this(3)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PinkPearls"/> class.
		/// </summary>
		public PinkPearls(int charges = 3) : base(3, charges)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200011)); /* [You are looking at] [a necklace of lustrous pink pearls.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250010)); /* The amulet contains the spell of Locate. */
		}
	}
}