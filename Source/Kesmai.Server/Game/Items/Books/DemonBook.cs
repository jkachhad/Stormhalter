using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DemonBook : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000011;

		/// <summary>
		/// Initializes a new instance of the <see cref="DemonBook"/> class.
		/// </summary>
		[WorldForge]
		public DemonBook() : base(192)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200311)); /* [You are looking at] [a dusty black book covered with strangely disturbing sigils.] */
		}
	}
}