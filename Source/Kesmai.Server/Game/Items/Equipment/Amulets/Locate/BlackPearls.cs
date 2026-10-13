using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class BlackPearls : LocateAmulet
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 600;
		
	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;
		
	public BlackPearls(IEntityProvider entityProvider, EntityDependencies entityDependencies) : this(entityProvider, entityDependencies, 3)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="BlackPearls"/> class.
	/// </summary>
	public BlackPearls(IEntityProvider entityProvider, EntityDependencies entityDependencies, int charges) : base(entityProvider, entityDependencies, 120, charges)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="BlackPearls"/> class.
	/// </summary>

	public BlackPearls(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
	{
	}
		
	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200069)); /* [You are looking at] [a necklace of lustrous black pearls.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250010)); /* The amulet contains the spell of Locate. */
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
