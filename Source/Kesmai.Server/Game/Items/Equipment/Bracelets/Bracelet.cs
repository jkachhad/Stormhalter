﻿using System.IO;

namespace Kesmai.Server.Items;

public abstract class Bracelet : Equipment
{
	/// <inheritdoc />
	public override int LabelNumber => 6000018;
		
	/// <inheritdoc />
	public override int Category => 7;

	/// <summary>
	/// Initializes a new instance of the <see cref="Bracelet"/> class.
	/// </summary>
	protected Bracelet(int braceletID) : base(braceletID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Bracelet"/> class.
	/// </summary>
	protected Bracelet(Serial serial) : base(serial)
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