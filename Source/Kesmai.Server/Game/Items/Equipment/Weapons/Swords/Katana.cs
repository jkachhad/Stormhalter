using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Katana : Sword
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000054;

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 1450;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Sword;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Slashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Katana"/> class.
		/// </summary>
		public Katana() : base(148)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200150)); /* [You are looking at] [a sharp steel katana.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250088)); /* The katana appears quite ordinary. */
		}
	}
}