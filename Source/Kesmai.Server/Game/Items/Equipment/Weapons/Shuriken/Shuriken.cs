using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Shuriken : Weapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000083;

		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 150;

		/// <inheritdoc />
		public override int Category => 14;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override Skill Skill => Skill.Throwing;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.QuickThrow | WeaponFlags.Throwable | WeaponFlags.Piercing;

		public override int ItemId
		{
			get
			{
				if (_poison != null)
					return PoisonedItemId;
					
				return base.ItemId;
			}
		}

		/// <summary>
		/// Gets the item id for this weapon when poisoned.
		/// </summary>
		protected virtual int PoisonedItemId => 315;

		/// <summary>
		/// Initializes a new instance of the <see cref="Shuriken"/> class.
		/// </summary>
		public Shuriken() : base(245)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200167)); /* [You are looking at] [a steel shuriken.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250093)); /* The shuriken appears quite ordinary. */
		}
	}
}