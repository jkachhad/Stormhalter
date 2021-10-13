using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SmallRuby : Gem
	{
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="SmallRuby"/> class.
		/// </summary>
		[WorldForge]
		public SmallRuby(uint basePrice) : base(140, basePrice)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200021)); /* [You are looking at] [a small red gem.] */
		}
	}
}
