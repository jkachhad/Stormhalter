using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public class RazLightningDragonScaleArmor : Armor, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000076; /* scales */

		/// <inheritdoc />
		public override uint BasePrice => 6400;

		/// <inheritdoc />
		public override int Weight => 1600;

		/// <inheritdoc />
		public override int Hindrance => 2;

		/// <inheritdoc />
		public override int SlashingProtection => 7;

		/// <inheritdoc />
		public override int PiercingProtection => 8;

		/// <inheritdoc />
		public override int BashingProtection => 7;

		/// <inheritdoc />
		public override int ProjectileProtection => 7;


		/// <inheritdoc />
		public override int ProtectionFromConcussion => 45;

		//TODO add lightning resistance instead of f/i.
		/// <inheritdoc />
		public override int ProtectionFromFire => 0;

		/// <inheritdoc />
		public override int ProtectionFromIce => 0;

		//todo add a different graphic for this.
		/// <summary>
		/// Initializes a new instance of the <see cref="RazLightningDragonScaleArmor"/> class.
		/// </summary>
		public RazLightningDragonScaleArmor() : base(219)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazLightningDragonScaleArmor"/> class.
		/// </summary>
		public RazLightningDragonScaleArmor(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500018)); /* [You are looking at] [a vest that crackles with electricity, made from the scales of a dragon.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500019)); /* The armor gives great protection against piercing and projectile attacks, as well as lightning. */
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

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(LightningResistanceStatus), out var resistStatus))
			{
				resistStatus = new LightningResistanceStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 50 }
				};
				resistStatus.AddSource(new ItemSource(this));

				entity.AddStatus(resistStatus);
			}
			else
			{
				resistStatus.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(LightningResistanceStatus), out var iceStatus))
				iceStatus.RemoveSourceFor(this);

			return true;
		}
	}
}