using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PlatemailGauntlets : Gauntlets, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000041;

		/// <inheritdoc />
		public override uint BasePrice => 100;

		/// <inheritdoc />
		public override int Weight => 200;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="PlatemailGauntlets"/> class.
		/// </summary>
		public PlatemailGauntlets() : base(37)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200169)); /* [You are looking at] [a pair of platemail gauntlets.] */
		}
	}
}