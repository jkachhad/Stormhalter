using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	//TODO: Change models and descriptions.
	public class RazStrengthRing : StrengthRing
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 1500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		public override int StrengthBonus => 9;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazStrengthRing"/> class.
		/// </summary>
		public RazStrengthRing() : base(53)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazStrengthRing"/> class.
		/// </summary>
		public RazStrengthRing(Serial serial) : base(serial)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200045)); /* [You are looking at] [a small gold ring with a glowing red gem.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500023)); /* The ring contains a very powerful spell of Strength. */
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