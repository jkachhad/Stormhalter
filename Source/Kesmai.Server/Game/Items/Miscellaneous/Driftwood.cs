using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PetrifiedWood : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000098;

		/// <inheritdoc />
		public override uint BasePrice => 700;

		/// <inheritdoc />
		public override int Weight => 15;

		/// <inheritdoc />
		public override int Category => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="PetrifiedWood"/> class.
		/// </summary>
		public PetrifiedWood() : base(13)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PetrifiedWood"/> class.
		/// </summary>
		public PetrifiedWood(Serial serial) : base(serial)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200328)); /* [You are looking at] [a piece of petrified wood.] */
		}
		
		/// <summary>
		/// Serializes this instance into binary data for persistence.
		/// </summary>
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1);	/* version */
		}

		/// <summary>
		/// Deserializes this instance from persisted binary data.
		/// </summary>
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