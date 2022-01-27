using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DragonFigurine : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000036;

		/// <summary>
		/// Initializes a new instance of the <see cref="DragonFigurine"/> class.
		/// </summary>
		[WorldForge]
		public DragonFigurine() : base(48)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200271)); /* [You are looking at] [a red figurine of a flying dragon.] */
		}
	}
}