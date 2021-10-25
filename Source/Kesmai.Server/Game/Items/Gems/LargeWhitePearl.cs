using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LargeWhitePearl : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000069;

		/// <summary>
		/// Initializes a new instance of the <see cref="LargeWhitePearl"/> class.
		/// </summary>
		[WorldForge]
		public LargeWhitePearl(uint basePrice) : base(349, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200295)); /* [You are looking at] [an enormous white pearl.] */
		}
	}
}