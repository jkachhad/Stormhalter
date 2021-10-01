using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class UncutDiamond : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="UncutDiamond"/> class.
		/// </summary>
		[WorldForge]
		public UncutDiamond(uint basePrice) : base(154, basePrice)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200124)); /* [You are looking at] [a large uncut diamond.] */
		}
	}
}