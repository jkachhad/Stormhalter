using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//todo decide if this is the treasure for wizards
	public class RazWizardStaff : Staff, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000088;

		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 1400;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 4;

		/// <inheritdoc />
		public override int BaseArmorBonus => 10;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Bashing | WeaponFlags.Lawful;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <inheritdoc />
		public override int ManaRegeneration => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazWizardStaff"/> class.
		/// </summary>
		public RazWizardStaff() : base(307)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazWizardStaff"/> class.
		/// </summary>
		public RazWizardStaff(Serial serial) : base(serial)
		{
		}

		public override bool CanUse(MobileEntity entity)
		{

			var flags = Flags;

			if (entity.LeftHand != null && flags.HasFlag(WeaponFlags.TwoHanded))
				return false; */


			if (!CanUse(entity.Alignment))
				return false;

			if (entity is PlayerEntity player && player.Profession is Wizard)
				return true;

			return false;
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500024)); /* [You are looking at] [a staff with ornate gems engraved into the surface. It feels as if it moves on its own.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500025)); /* The staff draws magic from the surrounding air. It seems honor bound to protect its wielder. */
		}

		/// <inheritdoc />
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt16();

			switch (version)
			{
				case 1:
					{
						break;
					}
			}
		}
	}
}