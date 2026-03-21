using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class GoldenEgg : Gem
{
	/// <inheritdoc />
	public override int LabelNumber => 6000033;
		
	/// <inheritdoc />
	public override int Weight => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="GoldenEgg"/> class.
	/// </summary>
	public GoldenEgg(uint basePrice) : base(191, basePrice)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="GoldenEgg"/> class.
	/// </summary>
	public GoldenEgg(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200263); /* [a small golden egg.] */
	}
}
