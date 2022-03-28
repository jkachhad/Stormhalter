using System;
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
		public override int SlashingMitigation => 1;

		/// <inheritdoc />
		public override int BashingMitigation => 1;
		
		/// <inheritdoc />
		public override int BaseArmorBonus
		{
			get
			{
				var baseArmorBonus = 1;
				
				/* Do not apply the armor bonus if on a hostile. */
				if (Parent is PlayerEntity)
					baseArmorBonus += Quality.Value; /* BaseArmorBonus matches rarity. */

				return Math.Min(baseArmorBonus, 3);
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
