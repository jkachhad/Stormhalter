using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class DemonWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 300;
		
		/// <inheritdoc />
		public override TargetFlags TargetFlags => TargetFlags.Mobiles;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(FearSpell);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DemonWand"/> class.
		/// </summary>
		public DemonWand() : base(207)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200226)); /* [You are looking at] [a stick carved with minute leering demon faces.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250115)); /* The wand contains the spell of Fear. */
		}
		
		public override Spell GetSpell()
		{
			return new FearSpell()
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

			if (spell is FearSpell fear)
			{
				fear.Warm(source);
				fear.CastAt(target);
			}
		}
	}
}