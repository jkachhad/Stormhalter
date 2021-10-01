using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public class RazSpeedRapier : Rapier, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 500;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 3;

		/// <inheritdoc />
		public override int MaximumDamage => 12;

		/// <inheritdoc />
		public override int BaseArmorBonus => 6;

		/// <inheritdoc />
		public override int BaseAttackBonus => 6;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing;

		/// <inheritdoc />
		public override bool CanBind => true;

		//todo pick a different graphic?
		/// <summary>
		/// Initializes a new instance of the <see cref="RazSpeedRapier"/> class.
		/// </summary>
		public RazSpeedRapier() : base(309)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazSpeedRapier"/> class.
		/// </summary>
		public RazSpeedRapier(Serial serial) : base(serial)
		{
		}

		//todo You are looking at an incredibly light rapier. A thrust from this weapon is nearly imperceptible.
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500014)); /* [You are looking at] [an impossibly thin black blade mounted on a silver hilt.  The rapier is emitting a faint blue glow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500015)); /* The rapier is forged with magic not of this land.*/
		}

		public override TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay(0.75);
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