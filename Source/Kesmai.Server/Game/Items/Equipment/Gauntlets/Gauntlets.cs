using System;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class Gauntlets : Equipment, IWeapon, IArmored
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000043; 

		/// <inheritdoc />
		public Skill Skill => Skill.Hand;

		/// <inheritdoc />
		public virtual WeaponFlags Flags => WeaponFlags.Bashing;

		/// <inheritdoc />
		public virtual int BaseArmorBonus => 0;

		/// <inheritdoc />
		public virtual int BaseAttackBonus => 0;

		/// <inheritdoc />
		public virtual int SlashingProtection => 0;

		/// <inheritdoc />
		public virtual int PiercingProtection => 0;

		/// <inheritdoc />
		public virtual int BashingProtection => 0;

		/// <inheritdoc />
		public virtual int ProjectileProtection => 0;

		/// <inheritdoc />
		public virtual int MinimumDamage => 0;

		/// <inheritdoc />
		public virtual int MaximumDamage => 0;

		/// <inheritdoc />
		public virtual ShieldPenetration Penetration => ShieldPenetration.None;

		/// <inheritdoc />
		public override int Category => 8;
		
		/// <inheritdoc />
		public override int AttackSound => 47;
		
		/// <inheritdoc />
		public int MaxRange => 0;

		public Poison Poison { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Gauntlets"/> class.
		/// </summary>
		protected Gauntlets(int glovesID) : base(glovesID)
		{
		}

		/// <inheritdoc />
		public override int GetFumbleLocalization()
		{
			return 6300017; /* The gauntlets leap off your hands. */
		}

		/// <summary>
		/// Gets the attack bonus provided by this <see cref="Gauntlets" /> for <see cref="MobileEntity" />.
		/// </summary>
		/// <remarks>
		/// Attack bonus provided by gloves is dependent on hand skill, hindrance, and <see cref="Penetration"/>.
		/// </remarks>
		public virtual int GetAttackBonus(MobileEntity entity)
		{
			return BaseAttackBonus;
		}

		/// <summary>
		/// Gets the swing delay for this <see cref="Gauntlets"/> for <see cref="MobileEntity"/>.
		/// </summary>
		public virtual TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay();
		}

		/// <summary>
		/// Calculates the blocking benefit of this instance against the specified item.
		/// </summary>
		public override int CalculateBlockingBonus(ItemEntity item)
		{
			if (item is IWeapon weapon && weapon.Flags.HasFlag(WeaponFlags.Projectile))
				return ProjectileProtection;

			return BaseArmorBonus;
		}
	}
}
