using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class IceProtectionRing : Ring, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 300;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		/// <summary>
		/// Initializes a new instance of the <see cref="IceProtectionRing"/> class.
		/// </summary>
		public IceProtectionRing() : base(30)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200060)); /* [You are looking at] [a dazzling diamond.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250047)); /* The ring contains the spell of Protection from Ice. */
		}
		
		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
			{
				iceStatus = new IceProtectionStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 42 }
				};
				iceStatus.AddSource(new ItemSource(this));
				
				entity.AddStatus(iceStatus);
			}
			else
			{
				iceStatus.AddSource(new ItemSource(this));
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;
			
			if (entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
				iceStatus.RemoveSourceFor(this);

			return true;
		}
	}
}