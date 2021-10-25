using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SphereDiamond : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="SphereDiamond"/> class.
		/// </summary>
		[WorldForge]
		public SphereDiamond(uint basePrice) : base(163, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200060)); /* [You are looking at] [a dazzling diamond.] */
		}
	}
}