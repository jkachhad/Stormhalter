using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class StrengthRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 1000;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int StrengthBonus => 3;

	public StrengthRing() : this(43)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="StrengthRing"/> class.
	/// </summary>
	public StrengthRing(int itemId) : base(itemId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="StrengthRing"/> class.
	/// </summary>
	public StrengthRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200044)); /* [You are looking at] [a gold ring with a large red gem set into it.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250034)); /* The ring contains a medium spell of Strength. */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (!entity.GetStatus(typeof(StrengthSpellStatus), out var status))
		{
			status = new StrengthSpellStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 53 }
			};
			status.AddSource(new ItemSource(this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ItemSource(this));
		}

		entity.Stats[EntityStat.Strength].Add(+StrengthBonus, ModifierType.Constant);
	}
	
	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		entity.Stats[EntityStat.Strength].Remove(+StrengthBonus, ModifierType.Constant);

		if (entity.GetStatus(typeof(StrengthSpellStatus), out var status))
			status.RemoveSource(this);
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