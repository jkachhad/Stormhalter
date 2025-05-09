using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class YouthPotion : Bottle, ITreasure
{
	private static ConsumableAmbrosia content = new ConsumableAmbrosia();
		
	/// <inheritdoc />
	public override uint BasePrice => 800;

	/// <inheritdoc />
	public override int Weight => 240;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="YouthPotion"/> class.
	/// </summary>
	public YouthPotion() : base(227)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="YouthPotion"/> class.
	/// </summary>
	public YouthPotion(Serial serial) : base(serial)
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
		entries.Add(new LocalizationEntry(6200000, 6200101)); /* [You are looking at] [a large silver bottle encrusted with sapphires.] */

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