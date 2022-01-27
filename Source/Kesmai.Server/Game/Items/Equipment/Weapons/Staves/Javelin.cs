using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Javelin : Staff
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000053;

		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override int Category => 2;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.MustThrow | WeaponFlags.Throwable | WeaponFlags.Piercing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Javelin"/> class.
		/// </summary>
		public Javelin() : base(190)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200161)); /* [You are looking at] [a silver colored javelin shaped like a lightning bolt.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250091)); /* The javelin casts a lightning bolt when thrown. */
		}
	}
}