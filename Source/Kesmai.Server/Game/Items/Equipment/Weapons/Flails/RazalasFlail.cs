using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	[WorldForge]
	public class RazVandalFlail : Flail, ITreasure
	{
		//todo can we place a strength enchant on this?
		/// <inheritdoc />
		public override int LabelNumber => 6000063;

		/// <inheritdoc />
		public override uint BasePrice => 3500;

		/// <inheritdoc />
		public override int Weight => 10800;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <inheritdoc />
		public override int MinimumDamage => 8;

		/// <inheritdoc />
		public override int MaximumDamage => 28;

		/// <inheritdoc />
		public override int BaseArmorBonus => 0;

		/// <inheritdoc />
		public override int BaseAttackBonus => 9;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.TwoHanded | WeaponFlags.Piercing | WeaponFlags.BlueGlowing | WeaponFlags.Bashing | WeaponFlags.Silver;

		/// <inheritdoc />
		public override bool CanBind => true;

		//todo add a new graphic for this.
		/// <summary>
		/// Initializes a new instance of the <see cref="RazVandalFlail"/> class.
		/// </summary>
		public RazVandalFlail() : base(197)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazVandalFlail"/> class.
		/// </summary>
		public RazVandalFlail(Serial serial) : base(serial)
		{
		}

		//todo You are looking at an impossibly heavy flail. To even pick it up takes tremendous effort.
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500020)); /* [You are looking at] [a heavy silver flail with a huge spiked ball on a steel chain. It gives off a blue glow.] */

			if (Identified)
				//todo: The flail's energy will sap your willpower but greatly increases your strength
				entries.Add(new LocalizationEntry(6500021)); /* The flail is a weapon of tremendous weight and power but drains the user to swing it.*/
		}

		public override TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay(2.0);
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