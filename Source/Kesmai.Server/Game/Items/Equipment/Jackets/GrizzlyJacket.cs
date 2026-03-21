using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class GrizzlyJacket : Jacket, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 40;

	/// <inheritdoc />
	public override int Weight => 2000;
        
	/// <inheritdoc />
	public override int Hindrance => 0;
		
	/// <inheritdoc />
	public override int ProtectionFromFire => 10;

	/// <summary>
	/// Initializes a new instance of the <see cref="GrizzlyJacket"/> class.
	/// </summary>
	public GrizzlyJacket() : base(273)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GrizzlyJacket"/> class.
	/// </summary>
	public GrizzlyJacket(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200204); /* [a coat made from the skin of an enormous grizzly bear.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250099); /* The jacket appears quite ordinary. */
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
