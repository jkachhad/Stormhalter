using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class FireProtectionAmulet : Amulet, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 500;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="FireProtectionAmulet"/> class.
		/// </summary>
		public FireProtectionAmulet() : base(4)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200065)); /* [You are looking at] [a diamond necklace with a glistening ruby.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250049)); /* The amulet contains the spell of Protection from Fire. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(FireProtectionStatus), out var status))
			{
				status = new FireProtectionStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 43 }
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

			if (entity.GetStatus(typeof(FireProtectionStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}
