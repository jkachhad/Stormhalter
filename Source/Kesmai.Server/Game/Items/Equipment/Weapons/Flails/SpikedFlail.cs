using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SpikedFlail : Flail
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000063;

		/// <inheritdoc />
		public override uint BasePrice => 35;

		/// <inheritdoc />
		public override int Weight => 1800;
		
		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 6;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SpikedFlail"/> class.
		/// </summary>
		public SpikedFlail() : base(197)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200166)); /* [You are looking at] [a heavy flail with a huge spiked ball on a steel chain.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250090)); /* The flail appears quite ordinary. */
		}
	}
}