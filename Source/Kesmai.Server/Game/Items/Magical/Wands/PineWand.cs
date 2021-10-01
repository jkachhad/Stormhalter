using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class PineWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;
		
		/// <inheritdoc />
		public override int Weight => 1000;
		
		/// <inheritdoc />
		public override Type ContainedSpell => typeof(FireballSpell);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PineWand"/> class.
		/// </summary>
		public PineWand() : base(281)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200228)); /* [You are looking at] [a pine wand inscribed with tiny red runes.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250117)); /* The wand contains the spell of Fireball. */
		}

		public override Spell GetSpell()
		{
			return new FireballSpell()
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

			if (spell is FireballSpell fireball)
			{
				fireball.Warm(source);
				fireball.CastAt(location);
			}
		}
	}
}