using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public class Boots : Equipment
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000012;
		
	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 11;

	/// <summary>
	/// Initializes a new instance of the <see cref="Boots"/> class.
	/// </summary>
	public Boots(IEntityProvider entityProvider, EntityDependencies entityDependencies, int bootsID) : base(entityProvider, entityDependencies, bootsID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Boots"/> class.
	/// </summary>

	public Boots(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial)
		: base(entityProvider, entityDependencies, serial)
	{
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
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
