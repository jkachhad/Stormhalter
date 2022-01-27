using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BloodRuby : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;
			
		/// <summary>
		/// Initializes a new instance of the <see cref="BloodRuby"/> class.
		/// </summary>
		[WorldForge]
		public BloodRuby(uint basePrice) : base(164, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200126)); /* [You are looking at] [a round, blood-red ruby.] */
		}
	}
}