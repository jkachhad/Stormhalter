using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Amethyst : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;
			
		/// <summary>
		/// Initializes a new instance of the <see cref="Amethyst"/> class.
		/// </summary>
		[WorldForge]
		public Amethyst(uint basePrice) : base(179, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200133)); /* [You are looking at] [an intensely violet amethyst.] */
		}
	}
}