using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	//TODO: new descriptions etc
	public class RazLightningStormOrb : SpellOrb, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 10000;

		/// <summary>
		/// Initializes a new instance of the <see cref="RazLightningStormOrb"/> class.
		/// </summary>
		public RazLightningStormOrb() : base(202)
		{
			Hue = Color.Yellow;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RazLightningStormOrb"/> class.
		/// </summary>
		public RazLightningStormOrb(Serial serial) : base(serial)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6500003)); /* [You are looking at] [a ball with a cloudy inside. You occasionally see a flash of lightning.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6500004)); /*The ball contains a powerful spell of Lightning Storm*/
		}

		/// <inheritdoc />
		protected override void PlaceEffect(MobileEntity source, Point2D location)
		{
			var spell = new LightningStormSpell()
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