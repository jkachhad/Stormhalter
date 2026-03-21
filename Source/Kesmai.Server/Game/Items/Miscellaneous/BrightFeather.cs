using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BrightFeather : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000101;

	/// <inheritdoc />
	public override uint BasePrice => 300;

	/// <inheritdoc />
	public override int Category => 3;

	/// <inheritdoc />
	public override bool CanBind => true;

	public BrightFeather() : base(397)
	{
	}
	
	public BrightFeather(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200285); /* [a bright feather.] */
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