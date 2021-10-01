using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public class RazReturningDagger : Dagger, IReturningWeapon, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.VeryHeavy;

		/// <inheritdoc />
		public override int MinimumDamage => 4;

		/// <inheritdoc />
		public override int MaximumDamage => 10;

		/// <inheritdoc />
		public override int BaseAttackBonus => 7;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.Returning | WeaponFlags.BlueGlowing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <inheritdoc />
		protected override int PoisonedItemId => 314;

		//todo make this a new dagger thing
		/// <summary>
		/// Initializes a new instance of the <see cref="RazReturningDagger"/> class.
		/// </summary>
		public RazReturningDagger() : base(308)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazReturningDagger"/> class.
		/// </summary>
		public RazReturningDagger(Serial serial) : base(serial)
		{
		}


		//todo You are looking at a dagger. It faintly glows blue
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500016)); /* [You are looking at] [a perfectly balanced dagger with a faint blue glow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500017)); /* The combat adds for this weapon are +6 */
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