using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SmallWhitePearl : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000069;

		/// <summary>
		/// Initializes a new instance of the <see cref="SmallWhitePearl"/> class.
		/// </summary>
		[WorldForge]
		public SmallWhitePearl(uint basePrice) : base(346, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200293)); /* [You are looking at] [a small white pearl.] */
		}
	}
}