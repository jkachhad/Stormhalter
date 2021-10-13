using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class HickoryWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 400;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(CreatePortalSpell);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="HickoryWand"/> class.
		/// </summary>
		public HickoryWand() : base(208)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200227)); /* [You are looking at] [a hickory wand with gold and silver inlay.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250116)); /* The wand contains the spell of Create Portal. */
		}
		
		public override Spell GetSpell()
		{
			return new CreatePortalSpell()
			{
				Item = this,
				
				Intensity = 2,
				SkillLevel = 8,
				
				Cost = 0,
			};
		}
		
		protected override void OnTarget(MobileEntity source, Point2D location)
		{
			var spell = GetSpell();

			if (spell is CreatePortalSpell portal)
			{
				portal.Warm(source);
				portal.CastAt(location);
			}
		}
	}
}