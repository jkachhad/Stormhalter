using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Jacket : Robe
{
	/// <inheritdoc />
	public override int LabelNumber => 6000052; /* jacket */
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Jacket"/> class.
	/// </summary>
	protected Jacket(IEntityProvider entityProvider, EntityDependencies entityDependencies, int jacketId) : base(entityProvider, entityDependencies, jacketId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Jacket"/> class.
	/// </summary>

	protected Jacket(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
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
