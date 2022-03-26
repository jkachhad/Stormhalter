using System.Collections.Generic;
using System.Linq;
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

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BaseArmorBonus { get { return 0; } }

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int SlashingMitigation { get { return 0; } }

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int PiercingMitigation { get { return 0; } }

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BashingMitigation { get { return 0; } }

		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ProjectileMitigation { get { return 0; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Shield"/> class.
		/// </summary>
		protected Shield(int shieldID) : base(shieldID)
		{
		}

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

		public virtual void OnWield(MobileEntity entity)
		{
		}
		
		public virtual void OnUnwield(MobileEntity entity)
		{
		}
	}
}
