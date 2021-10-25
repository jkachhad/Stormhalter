using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class OvalOnyx : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="OvalOnyx"/> class.
		/// </summary>
		[WorldForge]
		public OvalOnyx(uint basePrice) : base(345, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200291)); /* [You are looking at] [an onyx, cut into an oval shape.] */
		}
	}
}