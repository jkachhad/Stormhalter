using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class YellowBalm : Balm
{
	/// <summary>
	/// Initializes a new instance of the <see cref="YellowBalm"/> class.
	/// </summary>
	public YellowBalm() : base(211, 41)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="YellowBalm"/> class.
	/// </summary>
	public YellowBalm(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200088)); /* [You are looking at] [a clear bottle made of yellowish glass.] */

		base.GetDescription(entries);
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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