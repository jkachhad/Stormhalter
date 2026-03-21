using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class SteelLongsword : Longsword
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;
		
	/// <inheritdoc />
	public override int BaseAttackBonus => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="SteelLongsword"/> class.
	/// </summary>
	public SteelLongsword() : base(170)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SteelLongsword"/> class.
	/// </summary>
	public SteelLongsword(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200154); /* [a shiny steel longsword.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250003); /* The combat adds for this weapon are +4. */
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
