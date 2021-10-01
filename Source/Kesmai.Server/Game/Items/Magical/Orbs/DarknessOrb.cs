using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class DarknessOrb : SpellOrb, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 800;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DarknessOrb"/> class.
		/// </summary>
		public DarknessOrb() : base(202)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200275)); /* [You are looking at] [a black glass ball.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250122)); /* The ball contains the spell of darkness. */
		}

		/// <inheritdoc />
		protected override void PlaceEffect(MobileEntity source, Point2D location)
		{
			var spell = new DarknessSpell()
			{
				Item = this,
				
				Intensity = 2,
				SkillLevel = 12,
				
				Cost = 0,
			};

			spell.Warm(source);
			spell.CastAt(location);
		}
	}
}