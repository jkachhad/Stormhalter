using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class OakWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 800;

		/// <inheritdoc />
		public override TargetFlags TargetFlags => TargetFlags.Mobiles;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(DeathSpell);

		/// <summary>
		/// Initializes a new instance of the <see cref="OakWand"/> class.
		/// </summary>
		public OakWand() : base(205)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200224)); /* [You are looking at] [an oak wand adorned with black lines.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250113)); /* The wand contains the spell of Death. */
		}
		
		public override Spell GetSpell()
		{
			return new DeathSpell()
			{
				Item = this,
				
				Intensity = 2,
				SkillLevel = 8,
				
				Cost = 0,
			};
		}
		
		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			var spell = GetSpell();

			if (spell is DeathSpell death)
			{
				death.Warm(source);
				death.CastAt(target);
			}
		}
	}
}