using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class DogJacket : Jacket
{
	/// <inheritdoc />
	public override uint BasePrice => 20;

	/// <inheritdoc />
	public override int Weight => 1300;

	/// <summary>
	/// Initializes a new instance of the <see cref="DogJacket"/> class.
	/// </summary>
	public DogJacket() : base(254)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="DogJacket"/> class.
	/// </summary>
	public DogJacket(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200190); /* [a jacket made from the fur of a dog.] */

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
