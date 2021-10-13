using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PearDiamond : Gem
	{
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="PearDiamond"/> class.
		/// </summary>
		[WorldForge]
		public PearDiamond(uint basePrice) : base(153, basePrice)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200123)); /* [You are looking at] [an enormous pear cut diamond of the purest blue-white radiance.] */
		}
	}
}