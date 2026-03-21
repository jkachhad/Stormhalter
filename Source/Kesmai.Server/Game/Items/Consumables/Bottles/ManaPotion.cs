using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class ManaPotion : Bottle, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>
	public ManaPotion() : this(6, 24)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>
	public ManaPotion(int closedId, int openId) : base(closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>
	public ManaPotion(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = ConsumableRestoreMana.Full;
	}
		
	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200085); /* [a white porcelain bottle adorned with lotus blossoms.] */

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