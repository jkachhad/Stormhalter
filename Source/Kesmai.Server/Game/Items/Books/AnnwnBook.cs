﻿using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public partial class AnnwnBook : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int Weight => 5;

	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000011;

	/// <summary>
	/// Initializes a new instance of the <see cref="AnnwnBook"/> class.
	/// </summary>
	[WorldForge]
	public AnnwnBook() : base(298)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="AnnwnBook"/> class.
	/// </summary>
	public AnnwnBook(Serial serial) : base(serial)
	{
	}

	public override void Serialize(BinaryWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);
	}

	public override void Deserialize(BinaryReader reader)
	{
		base.Deserialize(reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200314)); /* [You are looking at] [a book describing a mysterious land called "Annwn".] */
	}
}