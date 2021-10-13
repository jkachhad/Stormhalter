using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class LightningShield : Shield, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 3000;

		/// <inheritdoc />
		public override int Category => 1;
		
		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int ProjectileProtection => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="LightningShield"/> class.
		/// </summary>
		public LightningShield() : base(280)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200215)); /* [You are looking at] [a steel shield adorned with a black lightning bolt.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250109)); /* The shield contains the spell of Lightning Resist. */
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
