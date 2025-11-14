using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class WeakStrengthRing : StrengthRing
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 500;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	public override int StrengthBonus => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="WeakStrengthRing"/> class.
	/// </summary>
	public WeakStrengthRing() : base(2)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="WeakStrengthRing"/> class.
	/// </summary>
	public WeakStrengthRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200010)); /* [You are looking at] [a gold ring with a small red gem set into it.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250009)); /* The ring contains a weak spell of Strength. */
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
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