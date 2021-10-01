using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	//TODO: item look
	public class RazConcOrb : SpellOrb, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 8000;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazConcOrb"/> class.
		/// </summary>
		public RazConcOrb() : base(202)
		{
			Hue = Color.Red;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazConcOrb"/> class.
		/// </summary>
		public RazConcOrb(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500000)); /* [You are looking at] [an unstable ball which vibrates in your hand. You feel uncomfortable holding it.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500001)); /* The ball contains a strong spell of concussion.*/
		}

		/// <inheritdoc />
		protected override void PlaceEffect(MobileEntity source, Point2D location)
		{
			var spell = new ConcussionSpell()
			{
				Item = this,

				SkillLevel = 15,

				Cost = 0,
			};

			spell.Warm(source);
			spell.CastAt(location);
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