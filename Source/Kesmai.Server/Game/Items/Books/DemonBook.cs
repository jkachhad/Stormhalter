using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class DemonBook : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000011;

	/// <summary>
	/// Initializes a new instance of the <see cref="DemonBook"/> class.
	/// </summary>
	public DemonBook() : base(192)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200311); /* [a dusty black book covered with strangely disturbing sigils.] */
	}
}