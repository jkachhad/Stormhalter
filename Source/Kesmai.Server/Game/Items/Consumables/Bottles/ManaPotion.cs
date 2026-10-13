using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public class ManaPotion : Bottle, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 500;

	/// <inheritdoc />
	public override int Weight => 240;

	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>

	public ManaPotion(IEntityProvider entityProvider, EntityDependencies entityDependencies)
		: this(entityProvider, entityDependencies, 6, 24)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>

	public ManaPotion(IEntityProvider entityProvider, EntityDependencies entityDependencies, int closedId, int openId)
		: base(entityProvider, entityDependencies, closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ManaPotion"/> class.
	/// </summary>
	public ManaPotion(IEntityProvider entityProvider, EntityDependencies entityDependencies, Serial serial) : base(entityProvider, entityDependencies, serial)
	{
	}
		
	/// <inheritdoc />
	protected override void OnCreate()
	{
		base.OnCreate();

		if (_content is null)
			_content = ConsumableRestoreMana.Full;
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200085)); /* [You are looking at] [a white porcelain bottle adorned with lotus blossoms.] */

		base.GetDescription(entries);
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
