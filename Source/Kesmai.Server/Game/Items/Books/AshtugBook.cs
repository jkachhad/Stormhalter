using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class AshtugBook : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000011;

	/// <summary>
	/// Initializes a new instance of the <see cref="AshtugBook"/> class.
	/// </summary>
	public AshtugBook() : base(155)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200309); /* [a small leather-bound book, with a title lettered on the cover. The book is slightly charred, and the title is faded beyond recognition.] */
	}
}