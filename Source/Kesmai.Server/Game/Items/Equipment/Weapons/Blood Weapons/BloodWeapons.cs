using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public abstract partial class BloodWeapon : MeleeWeapon, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000079;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Medium;

		/// <inheritdoc />
		public override int Category => 2;

		/// <inheritdoc />
		public override Skill Skill => Skill.Sword;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodWeapon" /> class.
		/// </summary>
		protected BloodWeapon(int swordID) : base(swordID)
		{
		}
	}

	public partial class BloodScythe : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000079;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override int Weight => 1400;

        /// <inheritdoc />
		public override int MinimumDamage => 2;

		/// <inheritdoc />
		public override int MaximumDamage => 12;

        /// <inheritdoc />
        public override bool CanBind => true;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override Skill Skill => Skill.Staff;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.TwoHanded | WeaponFlags.Slashing;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodScythe"/> class.
		/// </summary>
		public BloodScythe() : base(339)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodScythe"/> class.
		/// </summary>
		public BloodScythe(Serial serial) : base(serial)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200354)); /* [You are looking at] [a scythe with a blood groove leading all the way down to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodDagger : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000032;

		/// <inheritdoc />
		public override int Weight => 20;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inherit />
		public override Skill Skill => Skill.Dagger;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inherit />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		protected override int PoisonedItemId => 564;

		/// <inherit />
		public override WeaponFlags Flags => WeaponFlags.Piercing | WeaponFlags.Silver | WeaponFlags.Returning | WeaponFlags.BlueGlowing | WeaponFlags.Throwable | WeaponFlags.QuickThrow;

		/// <inherit />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodDagger"/> class.
		/// </summary>
		public BloodDagger() : base(308)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodDagger"/> class.
		/// </summary>
		public BloodDagger(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200355)); /* [You are looking at] [a sharp dagger with a blood groove that extends all the way to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodAxe : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000005;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override int Weight => 2560;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inherit />
		public override Skill Skill => Skill.Mace;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.BlueGlowing | WeaponFlags.Throwable | WeaponFlags.Slashing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodAxe"/> class.
		/// </summary>
		public BloodAxe() : base(381)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodAxe"/> class.
		/// </summary>
		public BloodAxe(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200356)); /* [You are looking at] [an extremely sharp axe with a blood groove that extends all the way to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodFlail : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000037;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override int Weight => 1800;

		/// <inherit />
		public override Skill Skill => Skill.Flail;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inherit />
		public override Skill Skill => Skill.Flail;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.TwoHanded | WeapongFlags.Piercing;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodFlail"/> class.
		/// </summary>
		public BloodFlail() : base(197)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodFlail"/> class.
		/// </summary>
		public BloodFlail(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200357)); /* [You are looking at] [a light flail with dozens of serrated spikes, seemingly ready to draw blood.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodPike : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000046;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inherit />
		public override Skill Skill => Skill.Halberd;

		/// <inheritdoc />
		public override int Weight => 5000;

		/// <inheritdoc />
		public override int MinimumDamage => 2;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Slashing | WeaponFlags.Piercing | WeaponFlags.TwoHanded;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodPike"/> class.
		/// </summary>
		public BloodPike() : base(173)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodPike"/> class.
		/// </summary>
		public BloodPike(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200358)); /* [You are looking at] [a pike with a wicked piercing tip and a blood groove flowing all the way down to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodRapier : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000071;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inherit />
		public override Skill Skill => Skill.Rapier;

		/// <inheritdoc />
		public override int Weight => 1000;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Piercing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodRapier"/> class.
		/// </summary>
		public BloodRapier() : base(309)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodRapier"/> class.
		/// </summary>
		public BloodRapier(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200359)); /* [You are looking at] [a rapier with a triple pointed tip and three blood grooves all the way to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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

	public partial class BloodSword : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000059;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inherit />
		public override Skill Skill => Skill.Sword;

		/// <inheritdoc />
		public override int Weight => 1000;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 3;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Piercing | WeaponFlags.Slashing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodSword"/> class.
		/// </summary>
		public BloodSword() : base(85)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodSword"/> class.
		/// </summary>
		public BloodSword(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200360)); /* [You are looking at] [a sword with a point designed for open wounds and two slashing edges. A blood groove flows down to the handle.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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
	public partial class BloodMace : BloodWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000103;

		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override int Weight => 5000;

		/// <inheritdoc />
		public override int Category => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Greatsword;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 14;

		/// <inheritdoc />
		public override int BaseArmorBonus => 4;

		/// <inheritdoc />
		public override int BaseAttackBonus => 5;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Bashing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodMace"/> class.
		/// </summary>
		public BloodMace() : base(338)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BloodMace"/> class.
		/// </summary>
		public BloodMace(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200361)); /* [You are looking at] [a massive two handed spiked warhammer.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250002)); /* The combat adds for this weapon are +5. */
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