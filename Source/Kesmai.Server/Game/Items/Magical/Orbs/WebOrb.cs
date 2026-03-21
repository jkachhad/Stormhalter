using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class WebOrb : SpellOrb, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 800;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="WebOrb"/> class.
	/// </summary>
	public WebOrb() : base(181)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WebOrb"/> class.
	/// </summary>
	public WebOrb(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200279); /* [a glass ball filled with a milky fluid.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250126); /* The ball contains the spell of create web. */
	}

	/// <inheritdoc />
	protected override void PlaceEffect(MobileEntity source, Point2D location)
	{
		var spell = new CreateWebSpell
		{
			Item = this,
				
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