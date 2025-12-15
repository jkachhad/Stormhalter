using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class IceStormOrb : SpellOrb, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1200;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="IceStormOrb"/> class.
	/// </summary>
	public IceStormOrb() : base(117)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="IceStormOrb"/> class.
	/// </summary>
	public IceStormOrb(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200276)); /* [You are looking at] [a small sphere of blue glass.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250123)); /* The ball contains the spell of icestorm. */
	}

	/// <inheritdoc />
	protected override void PlaceEffect(MobileEntity source, Point2D location)
	{
		var spell = new IceStormSpell
		{
			Item = this,
				
			Intensity = 2,
			SkillLevel = 12,
				
			Cost = 0,
		};

		spell.Warm(source);
		spell.CastAt(location);
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <inheritdoc />
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

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