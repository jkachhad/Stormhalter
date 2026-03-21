using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class GlassWand : Wand, ITreasure
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

	/// <summary>
	/// Initializes a new instance of the <see cref="GlassWand"/> class.
	/// </summary>
	public GlassWand(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200222); /* [a glass wand.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250111); /* The wand contains the spell of Ice Storm. */
	}

	public override Spell GetSpell()
	{
		return new IceStormSpell
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