using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class Mugwort : Food
{
	/* Mugwort cures to a absolute potency of 5. */
	private static ConsumablePoisonAntidote content = new ConsumablePoisonAntidote(5, false);
		
	/// <inheritdoc />
	public override int LabelNumber => 6000087;

	/// <inheritdoc />
	public override uint BasePrice => 25;

	/// <inheritdoc />
	public override int Weight => 100;

	/// <summary>
	/// Initializes a new instance of the <see cref="Mugwort"/> class.
	/// </summary>
	public Mugwort() : base(138)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Mugwort"/> class.
	/// </summary>
	public Mugwort(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = content;
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200231); /* [a dark-green sprig of mugwort.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250119); /* The sprig contains the spell of Neutralize. */
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
