using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LargeGoldPearl : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000069;

		/// <summary>
		/// Initializes a new instance of the <see cref="LargeGoldPearl"/> class.
		/// </summary>
		[WorldForge]
		public LargeGoldPearl(uint basePrice) : base(350, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200296)); /* [You are looking at] [an enormous golden pearl.] */
		}
	}
}