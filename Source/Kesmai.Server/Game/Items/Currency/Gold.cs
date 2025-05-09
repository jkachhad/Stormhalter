using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class Gold : Currency
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000028;

	/// <summary>
	/// Gets or sets the weight.
	/// </summary>
	public override int Weight => 1;

	/// <summary>
	/// Gets or sets the price.
	/// </summary>
	public override uint BasePrice => 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="Gold" /> class.
	/// </summary>
	public Gold() : this(1U)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Gold" /> class.
	/// </summary>
	public Gold(int amount) : this((uint)amount)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Gold"/> class.
	/// </summary>
	public Gold(uint amount = 1) : base(73)
	{
		Amount = amount;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Gold"/> class.
	/// </summary>
	public Gold(Serial serial) : base(serial)
	{
	}

	public override bool RespondsTo(string noun)
	{
		if (base.RespondsTo(noun))
			return true;

		return (noun.Matches("coins", true) || noun.Matches("gold", true));
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		if (Amount > 1)
			entries.Add(new LocalizationEntry(6199999, Amount.ToString(), 6199998)); /* You are looking at [#] [gold coins.]*/
		else
			entries.Add(new LocalizationEntry(6200000, 6200001)); /* [You are looking at] [a gold coin.]*/
	}
	
	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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