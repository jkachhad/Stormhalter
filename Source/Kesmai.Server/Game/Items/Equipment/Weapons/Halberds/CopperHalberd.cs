using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class CopperHalberd : Halberd, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 50;

		/// <inheritdoc />
		public override int Weight => 5000;

		/// <inheritdoc />
		public override int MinimumDamage => 2;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 2;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;
		
		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Lawful;

		/// <inheritdoc />
		public override bool CanBind => true;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="CopperHalberd"/> class.
		/// </summary>
		public CopperHalberd() : base(187)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200160)); /* [You are looking at] [a heavy halberd.  The pole is perfect ash and the blade is forged from a golden copper colored alloy.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
		}
	}
}