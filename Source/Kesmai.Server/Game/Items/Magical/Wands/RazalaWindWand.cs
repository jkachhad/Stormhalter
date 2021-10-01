using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	//TODO: new descriptions etc
	public class WindWand : Wand, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 1000;

		/// <inheritdoc />
		public override Type ContainedSpell => typeof(WhirlwindSpell);

		/// <summary>
		/// Initializes a new instance of the <see cref="WindWand"/> class.
		/// </summary>
		public PineWand() : base(281)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WindWand"/> class.
		/// </summary>
		public PineWand(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500026)); /* [You are looking at] [a wand inscribed with clouds.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500027)); /* The wand contains the spell of Whirlwind. */
		}

		public override Spell GetSpell()
		{
			return new WhirlwindSpell()
			{
				Item = this,

				SkillLevel = 17,

				Cost = 0,
			};
		}

		protected override void OnTarget(MobileEntity source, Point2D location)
		{
			var spell = GetSpell();

			if (spell is WhirlwindSpell fireball)
			{
				fireball.Warm(source);
				fireball.CastAt(location);
			}
		}

		/// <inheritdoc />
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

			var version = reader.ReadInt16();

			switch (version)
			{
				case 1:
					{
						break;
					}
			}
		}
	}
}