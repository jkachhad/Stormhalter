using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HorseStatue : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000051;

		/// <summary>
		/// Initializes a new instance of the <see cref="HorseStatue"/> class.
		/// </summary>
		[WorldForge]
		public HorseStatue() : base(12)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200303)); /* [You are looking at] [a small carved statue of a horse.] */
		}
	}
}