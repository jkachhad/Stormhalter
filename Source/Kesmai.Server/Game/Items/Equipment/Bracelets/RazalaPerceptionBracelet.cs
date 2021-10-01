using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public class RazPerceptionBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazPerceptionBracelet"/> class.
		/// </summary>
		public RazPerceptionBracelet() : base(11)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazPerceptionBracelet"/> class.
		/// </summary>
		public RazPerceptionBracelet(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500009)); /* [You are looking at] [a bracelet that focuses your vision directly to it.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500010)); /* The bracelet contains the spell of perception.*/
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

			if (!entity.GetStatus(typeof(PerceptionStatus), out var status))
			{
				status = new PerceptionStatus(entity)
				{
					//todo add proper SpellID
					Inscription = new SpellInscription() { SpellId = 4 }
				};
				status.AddSource(new ItemSource(this));

				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(PerceptionStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}