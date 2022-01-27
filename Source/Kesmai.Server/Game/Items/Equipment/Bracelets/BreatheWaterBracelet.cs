using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class BreatheWaterBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="BreatheWaterBracelet"/> class.
		/// </summary>
		public BreatheWaterBracelet() : base(11)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200041)); /* [You are looking at] [a silver bracelet set with tiny jade dolphins.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250031)); /* The bracelet contains the spell of Breathe Water. */
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