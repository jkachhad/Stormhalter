using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//TODO: Change models and descriptions.
	public class RazStrengthBracelet : StrengthBracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1500;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <inheritdoc />
		public override int StrengthBonus => 9;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazStrengthBracelet"/> class.
		/// </summary>
		public RazStrengthBracelet() : base(133)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazStrengthBracelet"/> class.
		/// </summary>
		public RazStrengthBracelet(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200082)); /* [You are looking at] [a silver bracelet engraved for decoration.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500022)); /* The bracelet contains a very powerful spell of Strength. */
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