using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class LightningResistanceRing : Ring, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2500;

		/// <inheritdoc />
		public override int Weight => 20;

		/// <summary>
		/// Initializes a new instance of the <see cref="LightningResistanceRing"/> class.
		/// </summary>
		public LightningResistanceRing() : base(27)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200058)); /* [You are looking at] [a steel ring of a sinuous dragon biting its own tail.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250046)); /* The ring contains the spell of Lightning Resist. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(LightningResistanceStatus), out var resistStatus))
			{
				resistStatus = new LightningResistanceStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 50 }
				};
				resistStatus.AddSource(new ItemSource(this));
				
				entity.AddStatus(resistStatus);
			}
			else
			{
				resistStatus.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;
			
			if (entity.GetStatus(typeof(LightningResistanceStatus), out var iceStatus))
				iceStatus.RemoveSourceFor(this);

			return true;
		}
	}
}
