using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SwiftGauntlets : Gauntlets, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 100;
		
		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 2;
		
		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.Neutral | WeaponFlags.Slashing;
		
		/// <inheritdoc />
		public override int ProjectileProtection => 1;
		
		public SwiftGauntlets() : base(396)
		{
		}

		/// <inheritdoc />
		public override bool CanEquip(MobileEntity entity)
		{
			if (entity is PlayerEntity player && player.Profession is Thief)
				return true;

			entity.SendMessage("The darkness of the gloves is tangible, but you can't seem to grasp it.");
			return false;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200284)); /* [You are looking at] [a pair of gloves that emanate strands of darkness.] */
		}

		public override TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay(0.5);
		}
	}
}