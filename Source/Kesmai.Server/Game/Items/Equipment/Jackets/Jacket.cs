using System.IO;

namespace Kesmai.Server.Items;

public abstract class Jacket : Robe
{
	/// <inheritdoc />
	public override int LabelNumber => 6000052; /* jacket */
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Jacket"/> class.
	/// </summary>
	protected Jacket(int jacketId) : base(jacketId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Jacket"/> class.
	/// </summary>
	protected Jacket(Serial serial) : base(serial)
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