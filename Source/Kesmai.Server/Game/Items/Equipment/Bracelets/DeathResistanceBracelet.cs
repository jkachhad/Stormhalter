using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class DeathResistanceBracelet : Bracelet, ITreasure
{
	/// <inheritdoc />
	public override uint BasePrice => 1500;

	/// <inheritdoc />
	public override int Weight => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="DeathResistanceBracelet"/> class.
	/// </summary>
	public DeathResistanceBracelet() : base(108)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="DeathResistanceBracelet"/> class.
	/// </summary>
	public DeathResistanceBracelet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200080)); /* [You are looking at] [a silver bracelet set with violet carbuncles.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250056)); /* The bracelet contains the spell of Death Resistance. */
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