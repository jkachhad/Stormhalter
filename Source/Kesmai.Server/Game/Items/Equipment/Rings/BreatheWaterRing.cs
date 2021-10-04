using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class BreatheWaterRing : Ring, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		/// <summary>
		/// Initializes a new instance of the <see cref="BreatheWaterRing"/> class.
		/// </summary>
		public BreatheWaterRing() : base(44)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200006)); /* [You are looking at] [a small silver ring with a shiny black stone.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250005)); /* The ring contains the spell of Breathe Water. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(BreatheWaterStatus), out var status))
			{
				status = new BreatheWaterStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 4 }
				};
				status.AddSource(new ItemSource(this));
				
				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(BreatheWaterStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}
