using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//TODO make this stone drop from corruption bosses - allows teleport to corrupter boss.
	public class RazCorruptedStone : ItemEntity, ITreasure
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000075;

		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 2160;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 3;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazCorruptedStone"/> class.
		/// </summary>
		public RazCorruptedStone() : base(97)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazCorruptedStone"/> class.
		/// </summary>
		public RazCorruptedStone(Serial serial) : base(serial)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500012)); /* [You are looking at] [a corrupted gem, you feel your vision distort as you examine it.] */
		}

		/// <summary>
		/// Serializes this instance into binary data for persistence.
		/// </summary>
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */
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