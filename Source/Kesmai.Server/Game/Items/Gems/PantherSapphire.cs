using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PantherSapphire : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="PantherSapphire"/> class.
		/// </summary>
		[WorldForge]
		public PantherSapphire(uint basePrice) : base(167, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200129)); /* [You are looking at] [an icy-blue sapphire cut into the shape of a stalking panther.] */
		}
	}
}