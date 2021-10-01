using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GoldNugget : Gem
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000075;
		
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="GoldNugget"/> class.
		/// </summary>
		[WorldForge]
		public GoldNugget(uint basePrice) : base(84, basePrice)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200120)); /* [You are looking at] [a gold nugget.] */
		}
	}
}