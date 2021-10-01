using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//TODO: Change models and descriptions.
	public class RazDexRing : Ring, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 15000;

		/// <inheritdoc />
		public override int Weight => 20;

		/// <summary>
		/// The dexterity bonus provided by this ring.
		/// </summary>
		public virtual int BonusDexterity => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="DexterityRing"/> class.
		/// </summary>
		public RazDexRing() : this(19)
		{
			Hue = Color.Green;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DexterityRing"/> class.
		/// </summary>
		public RazDexRing(int ringId) : base(ringId)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DexterityRing"/> class.
		/// </summary>
		public RazDexRing(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500002)); /* [You are looking at] [a finely crafted ring that practically jumps out of your hands.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250044)); /* The ring greatly increases dexterity. */
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