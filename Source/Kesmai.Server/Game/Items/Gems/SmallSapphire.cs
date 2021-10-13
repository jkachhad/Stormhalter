using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SmallSapphire : Gem
	{
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="SmallSapphire"/> class.
		/// </summary>
		[WorldForge]
		public SmallSapphire(uint basePrice) : base(60, basePrice)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200116)); /* [You are looking at] [a small blue sapphire.] */
		}
	}
}
