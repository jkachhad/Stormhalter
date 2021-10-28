using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GreenTippedFeather : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000035;

		/// <summary>
		/// Initializes a new instance of the <see cref="GreenTippedFeather"/> class.
		/// </summary>
		[WorldForge]
		public GreenTippedFeather() : base(24)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200305)); /* [You are looking at] [a large feather with a bright green tip.] */
		}
	}
}