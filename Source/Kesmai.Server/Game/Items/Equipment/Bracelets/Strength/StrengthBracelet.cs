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
	public partial class StrengthBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 4;
		
		/// <inheritdoc />
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int StrengthBonus => 3;

		/// <summary>
		/// Initializes a new instance of the <see cref="StrengthBracelet"/> class.
		/// </summary>
		public StrengthBracelet() : this(133)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="StrengthBracelet"/> class.
		/// </summary>
		public StrengthBracelet(int itemId) : base(itemId)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200082)); /* [You are looking at] [a silver bracelet engraved for decoration.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250058)); /* The bracelet contains a medium spell of Strength. */
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
