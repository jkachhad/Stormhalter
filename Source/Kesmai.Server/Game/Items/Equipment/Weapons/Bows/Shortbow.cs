using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Shortbow : Bow
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000015;

		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 600;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override int NockedID => 99;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Shortbow"/> class.
		/// </summary>
		public Shortbow() : base(228)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200034)); /* [You are looking at] [a shortbow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250026)); /* The shortbow appears quite ordinary. */
		}
	}
}