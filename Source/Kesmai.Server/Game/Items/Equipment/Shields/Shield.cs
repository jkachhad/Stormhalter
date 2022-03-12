using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Shield : ItemEntity, IArmored, IWieldable
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000082;

		/// <summary>
		/// Gets the base armor bonus provided by this <see cref="Armor"/>.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BaseArmorBonus { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against slashing attacks.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int SlashingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against peircing attacks.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int PiercingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against bashing attacks.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BashingProtection { get { return 0; } }

		/// <summary>
		/// Gets the protection provided against projectile attacks.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProjectileProtection { get { return 0; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Shield"/> class.
		/// </summary>
		protected Shield(int shieldID) : base(shieldID)
		{
		}

		public int GetMeleeBonus(ItemEntity item, WeaponFlags flags)
		{
			var protectionBonus = 0
			var itemProtections = new List<int>();

			if ((flags & WeaponFlags.Piercing) != 0)
				itemProtections.Add(PiercingProtection);

			if ((flags & WeaponFlags.Slashing) != 0)
				itemProtections.Add(SlashingProtection);

			if ((flags & WeaponFlags.Bashing) != 0)
				itemProtections.Add(BashingProtection);

			if (itemProtections.Any())
				protectionBonus += itemProtections.Min();

			return protectionBonus;
		}
		
		/// <inheritdoc/>
		/// <remarks>
		/// Shields only provide a blocking bonus against weapons when equipped in the left-hand.
		/// Used for DR calculations.
		/// </remarks?
		public override int CalculateBlockingBonus(ItemEntity item)
		{
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var bonusBlock = 0
			/// Let shields use BaseArmorBonus to avoid projectiles	
			if ((flags & WeaponFlags.Projectile) != 0)
				bonusBlock += ProjectileProtection;
			else
				bonusBlock += GetMeleeBonus(item, flags);

			return BaseArmorBonus + bonusBlock;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Shields only provide a blocking bonus against weapons when equipped in the left-hand.
		/// Used for mitigation calculations.
		/// </remarks?
		public int GetShieldBonus(ItemEntity item)
		{
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;

			var shieldBonus = 0;

			if ((flags & WeaponFlags.Projectile) != 0)
			{
				shieldBonus += ProjectileProtection;
			}
			else
			{
				shieldBonus += GetMeleeBonus(item, flags)
			}

			return shieldBonus + BaseArmorBonus;
		}

		public virtual void OnWield(MobileEntity entity)
		{
		}
		
		public virtual void OnUnwield(MobileEntity entity)
		{
		}
	}
}
