using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class UncutEmerald : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="UncutEmerald"/> class.
		/// </summary>
		[WorldForge]
		public UncutEmerald(uint basePrice) : base(178, basePrice)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200132)); /* [You are looking at] [a massive uncut emerald, as green as spring grass.] */
		}
	}
}