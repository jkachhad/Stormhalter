using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Shield : ItemEntity, IArmored
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000082;

		/// <summary>
		/// Gets the base armor bonus provided by this <see cref="Armor"/>.
		/// </summary>
		public virtual int BaseArmorBonus { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against slashing attacks.
		/// </summary>
		public virtual int SlashingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against peircing attacks.
		/// </summary>
		public virtual int PiercingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against bashing attacks.
		/// </summary>
		public virtual int BashingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against projectile attacks.
		/// </summary>
		public virtual int ProjectileProtection { get { return 0; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Shield"/> class.
		/// </summary>
		protected Shield(int shieldID) : base(shieldID)
		{
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Shields only provide a blocking bonus against weapons when equipped in the left-hand.
		/// </remarks>
		public override int CalculateBlockingBonus(ItemEntity item)
		{
			if (item is IWeapon weapon && weapon.Flags.HasFlag(WeaponFlags.Projectile))
				return ProjectileProtection;

			return BaseArmorBonus;
		}
	}
}
