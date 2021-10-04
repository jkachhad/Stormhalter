using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class StrengthRing : Ring, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 1000;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		public virtual int StrengthBonus => 3;

		public StrengthRing() : this(43)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StrengthRing"/> class.
		/// </summary>
		public StrengthRing(int itemId) : base(itemId)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200044)); /* [You are looking at] [a gold ring with a large red gem set into it.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250034)); /* The ring contains a medium spell of Strength. */
		}
		
		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(StrengthSpellStatus), out var status))
			{
				status = new StrengthSpellStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 53 }
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

			if (entity.GetStatus(typeof(StrengthSpellStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}
