using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Spear : Staff
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000086;

		/// <inheritdoc />
		public override uint BasePrice => 15;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Piercing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Spear"/> class.
		/// </summary>
		public Spear() : base(124)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200020)); /* [You are looking at] [a wooden spear with an iron tip.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250019)); /* The spear appears quite ordinary. */
		}
	}
}