using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HummingbirdAmulet : StunDeathProtectionAmulet
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 4500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;
		
		public HummingbirdAmulet() : this(6)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="HummingbirdAmulet"/> class.
		/// </summary>
		public HummingbirdAmulet(int charges = 6) : base(305, charges)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200073)); /* [You are looking at] [a crystal hummingbird hanging from a golden chain.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250052)); /* The amulet contains the spell of Protection from Stun and Death. */
		}
	}
}