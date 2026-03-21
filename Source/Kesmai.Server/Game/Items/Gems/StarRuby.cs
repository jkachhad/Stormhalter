using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class StarRuby : Gem
{
	/// <inheritdoc />
	public override int Weight => 5;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	public StarRuby() : this(500u)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	public StarRuby(uint basePrice) : base(90, basePrice)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="StarRuby"/> class.
	/// </summary>
	public StarRuby(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200121); /* [a highly polished star ruby with a peculiar glow eminating from its core.] */
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
