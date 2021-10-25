using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class TigerEye : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="TigerEye"/> class.
		/// </summary>
		[WorldForge]
		public TigerEye(uint basePrice) : base(196, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200134)); /* [You are looking at] [a rich, chocolate-brown tiger's eye streaked with amber bands.] */
		}
	}
}