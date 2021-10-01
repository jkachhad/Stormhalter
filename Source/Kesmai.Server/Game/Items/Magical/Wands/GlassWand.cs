using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class GlassWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 700;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(IceStormSpell);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="GlassWand"/> class.
		/// </summary>
		public GlassWand() : base(189)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200222)); /* [You are looking at] [a glass wand.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250111)); /* The wand contains the spell of Ice Storm. */
		}
		
		public override Spell GetSpell()
		{
			return new IceStormSpell()
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

			if (spell is IceStormSpell icestorm)
			{
				icestorm.Warm(source);
				icestorm.CastAt(location);
			}
		}
	}
}