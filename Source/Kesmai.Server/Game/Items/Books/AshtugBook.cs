using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class AshtugBook : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000011;

		/// <summary>
		/// Initializes a new instance of the <see cref="AshtugBook"/> class.
		/// </summary>
		[WorldForge]
		public AshtugBook() : base(155)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200309)); /* [You are looking at] [a small leather-bound book, with a title lettered on the cover. The book is slightly charred, and the title is faded beyond recognition.] */
		}
	}
}