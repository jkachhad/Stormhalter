using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Football : Mace
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000039;

		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 2000;

		/// <inheritdoc />
		public override int Category => 2;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.QuickThrow | WeaponFlags.Throwable | WeaponFlags.Bashing;

		/// <summary>
		/// Initializes a new instance of the <see cref="Football"/> class.
		/// </summary>
		public Football() : base(162)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200218)); /* [You are looking at] [a leather football.] */
		}
	}
}