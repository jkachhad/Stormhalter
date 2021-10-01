using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public class RazMAArmor : Armor, ITreasure
	{
		//todo add a label
		/// <inheritdoc />
		public override int LabelNumber => 6000076; /* scales */

		/// <inheritdoc />
		public override uint BasePrice => 8000;

		/// <inheritdoc />
		public override int Weight => 500;

		/// <inheritdoc />
		public override int Hindrance => 0;

		/// <inheritdoc />
		public override int SlashingProtection => 3;

		/// <inheritdoc />
		public override int PiercingProtection => 3;

		/// <inheritdoc />
		public override int BashingProtection => 3;

		/// <inheritdoc />
		public override int ProjectileProtection => 5;

		/// <inheritdoc />
		public override int ProtectionFromFire => 5;

		/// <inheritdoc />
		public override int ProtectionFromIce => 5;

		//todo add a new graphic for this.
		/// <summary>
		/// Initializes a new instance of the <see cref="RazMAArmor"/> class.
		/// </summary>
		public RazMAArmor() : base(243)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazMAArmor"/> class.
		/// </summary>
		public RazMAArmor(Serial serial) : base(serial)
		{
		}

		//todo you are looking at a suit of armor that feels lighter than the air itself.
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500007)); /* [You are looking at] [a suit of armor that feels lighter than the air itself.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500008)); /* The armor provides no hindrance and makes you feel light as a feather. */
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

		/// <inheritdoc />
		protected override bool OnEquip(MobileEntity entity)
		{
			

			var onEquip = base.OnEquip(entity);

			if (!entity.GetStatus(typeof(FeatherFallStatus), out var status))
			{
				status = new FeatherFallStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 14 }
				};
				status.AddSource(new ItemSource(this));

				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}
			
			if (entity is PlayerEntity player && player.Profession is MartialArtist)
				return true;

			entity.SendMessage("The armor recoils away from your bodies aura.");
			return false;

			return onEquip;
		}

		/// <inheritdoc />
		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(FeatherFallStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}