using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DungeonBook : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000011;

		/// <summary>
		/// Initializes a new instance of the <see cref="DungeonBook"/> class.
		/// </summary>
		[WorldForge]
		public DungeonBook() : base(296)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200312)); /* [You are looking at] [a small paper book entitled 'The Dungeon'.] */
		}
	}
}