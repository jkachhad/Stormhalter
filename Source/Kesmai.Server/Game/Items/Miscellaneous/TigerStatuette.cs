using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class TigerStatuette : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000036;

		/// <summary>
		/// Initializes a new instance of the <see cref="TigerStatuette"/> class.
		/// </summary>
		[WorldForge]
		public TigerStatuette() : base(50)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200306)); /* [You are looking at] [a minute ivory carving of a tiger.] */
		}
	}
}