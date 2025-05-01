using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class GreenBalm : Balm
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GreenBalm"/> class.
	/// </summary>
	public GreenBalm() : base(247, 94)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="GreenBalm"/> class.
	/// </summary>
	public GreenBalm(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200104)); /* [You are looking at] [a green glass bottle.] */

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