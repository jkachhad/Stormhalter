using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LeatherArmor : Armor
	{
		/* Add a new backing field */
		private int _baseArmorBonus;

		private Color _hue;
		
		private string _rarity;
		
		/// <inheritdoc />
		public override uint BasePrice => 25;

		/// <inheritdoc />
		public override int Weight => 1500;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int SlashingProtection => 1;

		/// <inheritdoc />
		public override int BashingProtection => 1;
		
		public override int BaseArmorBonus // This is now configured to what we set.
		{
			get
			{
				if (Owner is PlayerEntity)
					return _baseArmorBonus;

				return 0;
			}
		}

		public override Color Hue => _hue;
		
		
		/// <summary>
		/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
		/// </summary>
		public LeatherArmor(int armorBonus = 0) : base(242)
		{
			_baseArmorBonus = armorBonus;
			_hue = Rarity.GetRarityColor(armorBonus);
			_rarity = Rarity.GetRarity(armorBonus)
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200002)); /* [You are looking at] [a suit of leather armor.] */

			if (Identified)
				entries.Add(new LocalizationEntry($"This armor seems quite {_rarity}"));
			entries.Add(new LocalizationEntry($"This has +{_baseArmoronus} armor."));
		}
		
		
		/* This is pulled from the internal code to be exposed since we are modifying it */
		/// <inheritdoc />
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			/* increment the version so items get saved as new version. */
			//writer.Write((short)1); /* version */
			writer.Write((short)2); /* version */
			
			writer.Write((byte)_baseArmorBonus);
			//Do we need to Serialize/Deserialize the Hue?
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt16();
			
			switch (version)
			{
				// Add a new block here to load items into the right version, and convert to new version.
				case 2:
				{
					_baseArmorBonus = (int)reader.ReadByte();  // This property was added in version 2.

					goto case 1; // Load any data from the older version.
				}
				case 1:
				{
					break;
				}
			}
		}
	}
}
