using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class ClearBalm : Balm
{
	/// <inheritdoc />
	public override uint BasePrice => 16;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="ClearBalm"/> class.
	/// </summary>
	public ClearBalm() : base(210, 93)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ClearBalm"/> class.
	/// </summary>
	public ClearBalm(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200005); /* [a clear glass bottle.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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