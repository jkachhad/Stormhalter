using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class FireballOrb : SpellOrb, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1000;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FireballOrb"/> class.
		/// </summary>
		public FireballOrb() : base(200)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200278)); /* [You are looking at] [a red glass ball.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250125)); /* The ball contains the spell of fireball. */
		}

		/// <inheritdoc />
		protected override void PlaceEffect(MobileEntity source, Point2D location)
		{
			var spell = new FireballSpell()
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