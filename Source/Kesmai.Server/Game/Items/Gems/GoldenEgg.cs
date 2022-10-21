using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GoldenEgg : Gem
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000033;
		
		/// <inheritdoc />
		public override int Weight => 10;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldenEgg"/> class.
		/// </summary>
		[WorldForge]
		public GoldenEgg(uint basePrice) : base(191, basePrice)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200263)); /* [You are looking at] [a small golden egg.] */
		}
	}
}