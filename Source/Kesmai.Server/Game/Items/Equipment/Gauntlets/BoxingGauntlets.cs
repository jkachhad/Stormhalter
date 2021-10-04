using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BoxingGauntlets : Gauntlets
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000043;

		/// <inheritdoc />
		public override uint BasePrice => 50;

		/// <inheritdoc />
		public override int Weight => 200;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="BoxingGauntlets"/> class.
		/// </summary>
		public BoxingGauntlets() : base(70)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200171)); /* [You are looking at] [a pair of thickly padded leather boxing gloves.] */
		}
	}
}