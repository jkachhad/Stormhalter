using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class StunResistanceBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1200;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="StunResistanceBracelet"/> class.
		/// </summary>
		public StunResistanceBracelet() : base(10)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200077)); /* [You are looking at] [a jade bracelet.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250055)); /* The bracelet contains the spell of Stun Resistance. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(StunResistanceStatus), out var resistance))
			{
				resistance = new StunResistanceStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 51 }
				};
				resistance.AddSource(new ItemSource(this));
				
				entity.AddStatus(resistance);
			}
			else
			{
				resistance.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;
			
			if (entity.GetStatus(typeof(StunResistanceStatus), out var resistance))
				resistance.RemoveSourceFor(this);

			return true;
		}
	}
}
