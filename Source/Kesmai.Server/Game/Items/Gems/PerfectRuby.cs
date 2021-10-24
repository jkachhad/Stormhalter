using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PerfectRuby : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="PerfectRuby"/> class.
		/// </summary>
		[WorldForge]
		public PerfectRuby(uint basePrice) : base(359, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200299)); /* [You are looking at] [an exquisitely carved ruby.] */
		}
	}
}