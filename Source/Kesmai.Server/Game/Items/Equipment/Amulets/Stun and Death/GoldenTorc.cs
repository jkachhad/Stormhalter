using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GoldenTorc : StunDeathProtectionAmulet
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000092;

		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 1500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 150;
		
		public GoldenTorc() : this(2)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GoldenTorc"/> class.
		/// </summary>
		public GoldenTorc(int charges = 2) : base(300, charges)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200070)); /* [You are looking at] [a twisted gold torc inscribed with glowing runes.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250052)); /* The amulet contains the spell of Protection from Stun and Death. */
		}
	}
}