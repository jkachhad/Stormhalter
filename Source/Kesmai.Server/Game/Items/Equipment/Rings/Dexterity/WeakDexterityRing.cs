using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class WeakDexterityRing : DexterityRing, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int BonusDexterity => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="WeakDexterityRing"/> class.
	/// </summary>
	public WeakDexterityRing() : base(55)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WeakDexterityRing"/> class.
	/// </summary>
	public WeakDexterityRing(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200049); /* [a flexible ring of interwoven gold wire.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250040); /* The ring increases dexterity slightly. */
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
