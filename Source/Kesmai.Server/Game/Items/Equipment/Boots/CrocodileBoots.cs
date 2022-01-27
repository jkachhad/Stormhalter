using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class CrocodileBoots : Boots, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 480;

		/// <inheritdoc />
		public override int Hindrance => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="CrocodileBoots"/> class.
		/// </summary>
		public CrocodileBoots() : base(122)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200207)); /* [You are looking at] [a pair of boots made from crocodile hide.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250103)); /* The boots contain the spell of Breathe Water. */
		}

		/// <inheritdoc />
		protected override bool OnEquip(MobileEntity entity)
		{
			var onEquip = base.OnEquip(entity);

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

			return onEquip;
		}

		/// <inheritdoc />
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