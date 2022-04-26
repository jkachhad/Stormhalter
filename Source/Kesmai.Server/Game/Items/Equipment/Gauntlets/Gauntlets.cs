using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class Gauntlets : Equipment, IWeapon, IArmored
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000043; 

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public Skill Skill => Skill.Hand;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual WeaponFlags Flags => WeaponFlags.Bashing;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BaseArmorBonus => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BaseAttackBonus => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int SlashingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int PiercingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BashingMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProjectileMitigation => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int MinimumDamage => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int MaximumDamage => 0;

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int Category => 8;
		
		/// <inheritdoc />
		public override int AttackSound => 47;
		
		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
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
		public virtual double CalculateAttackBonus(MobileEntity attacker, MobileEntity defender)
		{
			var attackBonus = BaseAttackBonus + (double)Penetration;
			
			var skillLevel = attacker.GetSkillLevel(Skill.Hand);
			var hindrance = attacker.CalculateHindrance();
			var hindrancePenalty = (hindrance.Total * skillLevel) * 0.25;

			return attackBonus - hindrancePenalty;
		}

		/// <summary>
		/// Gets the swing delay for this <see cref="Gauntlets"/> for <see cref="MobileEntity"/>.
		/// </summary>
		public virtual TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay();
		}
		
		/// <summary>
		/// Gets the multiplier for skill gain awarded per weapon swing.
		/// </summary>
		public virtual double GetSkillMultiplier()
		{
			return 1.0;
		}

		/// <inheritdoc/>
		public int CalculateMitigationBonus(ItemEntity item)
		{
			var itemProtections = new List<int>();

			/* Determine the type of weapon attacking us. For hands, we use bashing damage. */
			var flags = WeaponFlags.Bashing;

			if (item is IWeapon weapon)
				flags = weapon.Flags;
			
			/* Determine mitigation. */
			var protectionBonus = 0;
			
			if ((flags & WeaponFlags.Projectile) != 0)
				itemProtections.Add(ProjectileMitigation);

			if ((flags & WeaponFlags.Piercing) != 0)
				itemProtections.Add(PiercingMitigation);

			if ((flags & WeaponFlags.Slashing) != 0)
				itemProtections.Add(SlashingMitigation);

			if ((flags & WeaponFlags.Bashing) != 0)
				itemProtections.Add(BashingMitigation);

			if (itemProtections.Any())
				protectionBonus += itemProtections.Min();

			return protectionBonus;
		}
		
		/// <inheritdoc/>
		public override int CalculateBlockingBonus(ItemEntity item)
		{
			return BaseArmorBonus;
		}
		
		/// <summary>
		/// Gauntlets at this time do not provide mitigation against weapons.
		/// </summary>
		public int GetWeaponBonus(ItemEntity item)
		{
			return 0;
		}
		
		/// <summary>
		/// Protection Bonus vs. Melee Damage Types
		/// </summary>
		public int GetMeleeBonus(ItemEntity item, WeaponFlags flags)
		{
			return 0;
		}
		
		public virtual void OnHit(MobileEntity attacker, MobileEntity defender)
		{
		}

		/// <summary>
		/// Overridable. Determines whether the specified instance can use this item.
		/// </summary>
		public override bool CanUse(MobileEntity entity)
		{
			if (!base.CanUse(entity))
				return false;

			/* We prevent the weapon from being beneficial if alignment values do not match. */
			var flags = Flags;
			var alignment = entity.Alignment;

			if ((flags.HasFlag(WeaponFlags.Lawful) && alignment != Alignment.Lawful) ||
			    (flags.HasFlag(WeaponFlags.Neutral) && alignment != Alignment.Neutral) ||
			    (flags.HasFlag(WeaponFlags.Chaotic) && alignment != Alignment.Chaotic && alignment != Alignment.Evil))
				return false;
			
			return true;
		}
	}
}
