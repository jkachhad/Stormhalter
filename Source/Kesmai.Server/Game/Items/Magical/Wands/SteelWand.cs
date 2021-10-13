using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class SteelWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;
		
		/// <inheritdoc />
		public override int Weight => 1000;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(LightningBoltSpell);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SteelWand"/> class.
		/// </summary>
		public SteelWand() : base(204)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200223)); /* [You are looking at] [a steel wand.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250112)); /* The wand contains the spell of Lightning Bolt. */
		}
		
		public override Spell GetSpell()
		{
			return new LightningBoltSpell()
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

			if (spell is LightningBoltSpell lightningBolt)
			{
				lightningBolt.Warm(source);
				lightningBolt.CastAt(location);
			}
		}
	}
}