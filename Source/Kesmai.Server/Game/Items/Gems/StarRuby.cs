using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class StarRuby : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StarRuby"/> class.
		/// </summary>
		public StarRuby() : this(500u)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StarRuby"/> class.
		/// </summary>
		[WorldForge]
		public StarRuby(uint basePrice) : base(90, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200121)); /* [You are looking at] [a highly polished star ruby with a peculiar glow eminating from its core.] */
		}
	}
}