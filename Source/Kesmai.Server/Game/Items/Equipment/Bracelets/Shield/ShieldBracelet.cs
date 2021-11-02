using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class ShieldBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 4;
		
		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Shield => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShieldBracelet"/> class.
		/// </summary>
		public ShieldBracelet() : this(7)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ShieldBracelet"/> class.
		/// </summary>
		public ShieldBracelet(int braceletId) : base(braceletId)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200074)); /* [You are looking at] [a silver bracelet made of scales.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250127)); /* The bracelet contains a medium spell of Shield. */
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
