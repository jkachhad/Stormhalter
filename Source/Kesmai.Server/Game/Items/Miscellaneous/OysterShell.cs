using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class OysterShell : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000081;

	/// <summary>
	/// Initializes a new instance of the <see cref="OysterShell"/> class.
	/// </summary>
	[WorldForge]
	public OysterShell() : base(52)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="OysterShell"/> class.
	/// </summary>
	public OysterShell(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200287)); /* [You are looking at] [an oyster shell.] */
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