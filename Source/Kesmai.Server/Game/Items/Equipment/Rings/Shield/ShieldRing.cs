using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class ShieldRing : Ring, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 750;
		
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		/// <summary>
		/// Gets or sets the shield-value provided by this ring.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Shield => 3;

		public ShieldRing() : this(45)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ShieldRing"/> class.
		/// </summary>
		public ShieldRing(int ringId) : base(ringId)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200046)); /* [You are looking at] [a small iron ring with a large black stone.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250128)); /* The ring contains a medium spell of Shield. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (!entity.GetStatus(typeof(ShieldStatus), out var status))
			{
				status = new ShieldStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 52 }
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

			if (entity.GetStatus(typeof(ShieldStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
	}
}
