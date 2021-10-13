using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlackPearls : LocateAmulet
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 600;
		
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;
		
		public BlackPearls() : this(3)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="BlackPearls"/> class.
		/// </summary>
		public BlackPearls(int charges) : base(120, charges)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200069)); /* [You are looking at] [a necklace of lustrous black pearls.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250010)); /* The amulet contains the spell of Locate. */
		}
	}
}