using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class FireIceProtectionRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 800;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="FireIceProtectionRing"/> class.
	/// </summary>
	public FireIceProtectionRing() : base(29)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="FireIceProtectionRing"/> class.
	/// </summary>
	public FireIceProtectionRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200048)); /* [You are looking at] [a gold ring set with four small diamonds encircling a fiery ruby.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250038)); /* The ring contains the spell of Protection from Fire and Ice. */
	}

	protected override bool OnEquip(MobileEntity entity)
	{
		if (!base.OnEquip(entity))
			return false;

		if (!entity.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
		{
			fireStatus = new FireProtectionStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 43 }
			};
			fireStatus.AddSource(new ItemSource(this));
				
			entity.AddStatus(fireStatus);
		}
		else
		{
			fireStatus.AddSource(new ItemSource(this));
		}
			
		if (!entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
		{
			iceStatus = new IceProtectionStatus(entity)
			{
				Inscription = new SpellInscription { SpellId = 42 }
			};
			iceStatus.AddSource(new ItemSource(this));
				
			entity.AddStatus(iceStatus);
		}
		else
		{
			iceStatus.AddSource(new ItemSource(this));
		}

		return true;
	}

	protected override bool OnUnequip(MobileEntity entity)
	{
		if (!base.OnUnequip(entity))
			return false;

		if (entity.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
			fireStatus.RemoveSource(this);
			
		if (entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
			iceStatus.RemoveSource(this);

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