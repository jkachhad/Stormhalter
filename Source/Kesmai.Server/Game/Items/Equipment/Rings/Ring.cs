using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract partial class Ring : ItemEntity
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000073;

	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="Ring"/> class.
	/// </summary>
	protected Ring(int ringID) : base(ringID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Ring"/> class.
	/// </summary>
	protected Ring(Serial serial) : base(serial)
	{
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