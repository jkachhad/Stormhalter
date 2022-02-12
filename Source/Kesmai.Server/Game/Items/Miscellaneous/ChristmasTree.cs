using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class ChristmasTree : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000093;

		/// <inheritdoc />
		public override uint BasePrice => 500000;

		/// <inheritdoc />
		public override int Weight => 15;

		/// <inheritdoc />
		public override int Category => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChristmasTree"/> class.
		/// </summary>
		public ChristmasTree() : base(324)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PetrifiedWood"/> class.
		/// </summary>
		public ChristmasTree(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200339)); /* [You are looking at] [a miniature decorative holiday tree.] */
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