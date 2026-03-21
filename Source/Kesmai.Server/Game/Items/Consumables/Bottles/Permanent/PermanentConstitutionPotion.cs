using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class PermanentConstitutionPotion : Bottle, ITreasure
{
	private static ConsumableConstitutionStat content = new ConsumableConstitutionStat();
		
	/// <inheritdoc />
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int Weight => 240;
	
	/// <inheritdoc />
	public override ItemQuality Quality => ItemQuality.Uncommon;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PermanentConstitutionPotion"/> class.
	/// </summary>
	public PermanentConstitutionPotion() : base(303)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PermanentConstitutionPotion"/> class.
	/// </summary>
	public PermanentConstitutionPotion(Serial serial) : base(serial)
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
		yield return LocalizationEntry.Get(6200113); /* [a small porcelain vial.] */

		foreach (var entry in base.AddDescriptionProperty(tooltip, beholder))
			yield return entry;
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