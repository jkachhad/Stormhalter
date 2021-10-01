using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//TODO make this an egg that completes a quest. "Get the wealthy man an egg of the hippogryph matriarch and me it's feather and I'll craft something that will make you feel light as a feather! MaxWeight += 140000?
	public class RazGriffinEgg : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000033;

		/// <inheritdoc />
		public override uint BasePrice => 70000;

		/// <inheritdoc />
		public override int Weight => 15;

		/// <inheritdoc />
		public override int Category => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazGriffinEgg"/> class.
		/// </summary>
		public RazGriffinEgg() : base(169)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazGriffinEgg"/> class.
		/// </summary>
		public RazGriffinEgg(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500013)); /* [You are looking at] [a majestic griffon egg. You suspect a collector might really value this.] */
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