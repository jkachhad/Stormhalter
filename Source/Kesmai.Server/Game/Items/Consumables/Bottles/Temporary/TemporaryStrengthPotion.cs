using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public class TemporaryStrengthPotion : Bottle, ITreasure
{
	private static ConsumableStrengthSpell content = new ConsumableStrengthSpell();
		
	/// <inheritdoc />
	public override uint BasePrice => 200;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryStrengthPotion"/> class.
	/// </summary>
	public TemporaryStrengthPotion() : this(211, 41)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryStrengthPotion"/> class.
	/// </summary>
	public TemporaryStrengthPotion(int closedId, int openId) : base(closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="TemporaryStrengthPotion"/> class.
	/// </summary>
	public TemporaryStrengthPotion(Serial serial) : base(serial)
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
		yield return LocalizationEntry.Get(6200090); /* [a clear bottle made of yellowish glass.] [Inside is a clear red liquid.] */

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