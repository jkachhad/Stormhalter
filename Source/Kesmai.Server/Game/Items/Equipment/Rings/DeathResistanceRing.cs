using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class DeathResistanceRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 300;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeathResistanceRing"/> class.
	/// </summary>
	public DeathResistanceRing() : base(18)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="DeathResistanceRing"/> class.
	/// </summary>
	public DeathResistanceRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200057)); /* [You are looking at] [a shiny gold ring set with a tiny bejeweled scarab.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250045)); /* The ring contains the spell of Death Resistance. */
	}

	protected override bool OnEquip(MobileEntity entity)
	{
		if (!base.OnEquip(entity))
			return false;

		if (!entity.GetStatus(typeof(DeathResistanceStatus), out var resistance))
		{
			resistance = new DeathResistanceStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 48 }
			};
			resistance.AddSource(new ItemSource(this));
				
			entity.AddStatus(resistance);
		}
		else
		{
			resistance.AddSource(new ItemSource(this));
		}

		return true;
	}

	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;
			
		if (entity.GetStatus(typeof(DeathResistanceStatus), out var resistance))
			resistance.RemoveSource(this);

		return true;
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