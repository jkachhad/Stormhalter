using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Longbow : Bow
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000058;

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 800;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 6;

		/// <inheritdoc />
		public override int NockedID => 113;

		/// <summary>
		/// Initializes a new instance of the <see cref="Longbow"/> class.
		/// </summary>
		public Longbow() : base(229)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200035)); /* [You are looking at] [a yew longbow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250027)); /* The longbow appears quite ordinary. */
		}
	}
}
