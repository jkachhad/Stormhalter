using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HeartShapedRuby : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="HeartShapedRuby"/> class.
		/// </summary>
		[WorldForge]
		public HeartShapedRuby(uint basePrice) : base(321, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200301)); /* [You are looking at] [a flawless ruby cut into the shape of a heart.] */
		}
	}
}