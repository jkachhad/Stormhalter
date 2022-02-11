using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Halberd : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000046;

		/// <inheritdoc />
		public override uint BasePrice => 40;

		/// <inheritdoc />
		public override int Weight => 4800;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Medium;

		/// <inheritdoc />
		public override int Category => 15;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 12;

		/// <inheritdoc />
		public override int BaseArmorBonus => 2;

		/// <inheritdoc />
		public override Skill Skill => Skill.Halberd;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Slashing | WeaponFlags.Bashing;

		/// <summary>
		/// Initializes a new instance of the <see cref="Halberd" /> class.
		/// </summary>
		public Halberd() : this(158)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Halberd" /> class.
		/// </summary>
		public Halberd(int halberdID) : base(halberdID)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200017)); /* [You are looking at] [a halberd with a steel head.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250016)); /* The halberd appears quite ordinary. */
		}
	}
}
