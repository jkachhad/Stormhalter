using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Diamond : Gem
	{
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="Diamond"/> class.
		/// </summary>
		[WorldForge]
		public Diamond(uint basePrice) : base(136, basePrice)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200122)); /* [You are looking at] [a large, flawless white diamond.] */
		}
	}
}