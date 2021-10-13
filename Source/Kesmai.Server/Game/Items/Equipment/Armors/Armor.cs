using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Armor : Equipment, IArmored
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000004; /* armor */
		
		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 9;
		
		/// <inheritdoc />
		public override int Hindrance => 1;
		
		#region IArmored
		
		/// <inheritdoc />
		public virtual int BaseArmorBonus => 0;

		/// <inheritdoc />
		public virtual int SlashingProtection => 0;

		/// <inheritdoc />
		public virtual int PiercingProtection => 0;

		/// <inheritdoc />
		public virtual int BashingProtection => 0;

		/// <inheritdoc />
		public virtual int ProjectileProtection => 0;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="Armor"/> class.
		/// </summary>
		protected Armor(int armorID) : base(armorID)
		{
		}
		
		/// <summary>
		/// Gets the armor bonus against the specified <see cref="ItemEntity"/>.
		/// </summary>
		public int GetArmorBonus(ItemEntity item)
		{
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var armorBonus = 0;

			if ((flags & WeaponFlags.Projectile) != 0)
			{
				armorBonus += ProjectileProtection;
			}
			else
			{
				if ((flags & WeaponFlags.Piercing) != 0)
					armorBonus += PiercingProtection;

				if ((flags & WeaponFlags.Slashing) != 0)
					armorBonus += SlashingProtection;

				if ((flags & WeaponFlags.Bashing) != 0)
					armorBonus += BashingProtection;
			}

			return armorBonus + BaseArmorBonus;
		}
	}
}
