﻿using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class PermanentWillpowerPotion : Bottle, ITreasure
{
	private static ConsumableWillpowerStat content = new ConsumableWillpowerStat();
		
	/// <inheritdoc />
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int Weight => 240;
	
	/// <inheritdoc />
	public override ItemQuality Quality => ItemQuality.Uncommon;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="PermanentWillpowerPotion"/> class.
	/// </summary>
	public PermanentWillpowerPotion() : base(289)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="PermanentWillpowerPotion"/> class.
	/// </summary>
	public PermanentWillpowerPotion(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = content;
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200110)); /* [You are looking at] [a porcelain bottle with small irises in raised relief around its base.] */

		base.GetDescription(entries);
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