using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class IronRod : Staff, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 25;

		/// <inheritdoc />
		public override int Weight => 4500;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Medium;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 6;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 3;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="IronRod"/> class.
		/// </summary>
		public IronRod() : base(72)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200008)); /* [You are looking at] [a long black iron rod with glowing red gems arranged in a ring near the center.  The weapon is lawful.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250007)); /* The combat adds for this weapon are +3. */
		}
	}
}