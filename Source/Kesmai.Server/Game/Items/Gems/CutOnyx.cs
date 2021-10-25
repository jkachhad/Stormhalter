using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class CutOnyx : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="CutOnyx"/> class.
		/// </summary>
		[WorldForge]
		public CutOnyx(uint basePrice) : base(358, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200298)); /* [You are looking at] [an onyx, expertly cut.] */
		}
	}
}