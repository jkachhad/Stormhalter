using System;
using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Food : Consumable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Food"/> class.
	/// </summary>
	public Food(int foodId) : base(foodId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Food"/> class.
	/// </summary>
	public Food(Serial serial) : base(serial)
	{
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