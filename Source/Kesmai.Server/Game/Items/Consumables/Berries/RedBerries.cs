using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public class RedBerries : Food
{
	private static ConsumableHeal content = new ConsumableHeal(5);
		
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 5;
		
	/// <inheritdoc />
	public override int LabelNumber => 6000008;

	/// <summary>
	/// Initializes a new instance of the <see cref="RedBerries"/> class.
	/// </summary>
	public RedBerries() : base(26)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="RedBerries"/> class.
	/// </summary>
	public RedBerries(Serial serial) : base(serial)
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