using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class ShieldBracelet : Bracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1000;

	/// <inheritdoc />
	public override int Weight => 4;
		
	/// <inheritdoc />
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Shield => 3;

	/// <summary>
	/// Initializes a new instance of the <see cref="ShieldBracelet"/> class.
	/// </summary>
	public ShieldBracelet() : this(7)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="ShieldBracelet"/> class.
	/// </summary>
	public ShieldBracelet(int braceletId) : base(braceletId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="ShieldBracelet"/> class.
	/// </summary>
	public ShieldBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200074)); /* [You are looking at] [a silver bracelet made of scales.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250127)); /* The bracelet contains a medium spell of Shield. */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (!entity.GetStatus(typeof(ShieldStatus), out var status))
		{
			status = new ShieldStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 52 }
			};
			status.AddSource(new ItemSource(this));
				
			entity.AddStatus(status);
		}
		else
		{
			status.AddSource(new ItemSource(this));
		}
		
		entity.Stats[EntityStat.Barrier].Add(+Shield, ModifierType.Constant);
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (entity.GetStatus(typeof(ShieldStatus), out var status))
			status.RemoveSource(this);
		
		entity.Stats[EntityStat.Barrier].Remove(+Shield, ModifierType.Constant);
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