using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LeatherArmor : Armor
	{
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
		
		public override int BaseArmorBonus
		{
			get
			{
				/* Do not apply the armor bonus if on a hostile. */
				if (Parent is PlayerEntity)
					return Quality.Value; /* BaseArmorBonus matches rarity. */

				return 0;
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
		/// </summary>
		public LeatherArmor() : this(ItemQuality.Common)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
		/// </summary>
		public LeatherArmor(ItemQuality quality) : base(242)
		{
			if (quality is null)
				quality = ItemQuality.Common;

			_armorQuality = quality;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200002)); /* [You are looking at] [a suit of leather armor.] */
		}
	}
}
