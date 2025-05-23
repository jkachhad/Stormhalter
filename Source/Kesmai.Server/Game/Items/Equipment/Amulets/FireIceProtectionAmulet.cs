using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class FireIceProtectionAmulet : Amulet, ITreasure, ICharged
{
	private int _chargesCurrent;
	private int _chargesMax;

	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public int ChargesCurrent
	{
		get => _chargesCurrent;
		set => _chargesCurrent = value.Clamp(0, _chargesMax);
	}
		
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public int ChargesMax
	{
		get => _chargesMax;
		set => _chargesMax = value;
	}
		
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 800;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;

	/// <summary>
	/// Initializes a new instance of the <see cref="FireIceProtectionAmulet"/> class.
	/// </summary>
	public FireIceProtectionAmulet() : this(3)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="FireIceProtectionAmulet"/> class.
	/// </summary>
	public FireIceProtectionAmulet(int charges) : base(5)
	{
		_chargesCurrent = charges;
		_chargesMax = charges;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="FireIceProtectionAmulet"/> class.
	/// </summary>
	public FireIceProtectionAmulet(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (_chargesCurrent > 0)
		{
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
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (entity.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
			fireStatus.RemoveSource(this);
			
		if (entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
			iceStatus.RemoveSource(this);
	}
		
	public override void OnStrip(Corpse corpse)
	{
		/* Only reduce charges if the item was stripped when on paperdoll or rings. */
		if (Container is EquipmentContainer)
		{
			if (_chargesCurrent > 0)
				_chargesCurrent--;
		}
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200066)); /* [You are looking at] [a gold necklace with sparkling sapphires.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250050)); /* The amulet contains the spell of Protection from Fire and Ice. */
	}
	
	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)2); /* version */
			
		writer.Write(_chargesMax);
		writer.Write(_chargesCurrent);
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
			case 2:
			{
				_chargesMax = reader.ReadInt32();
				goto case 1;
			}
			case 1:
			{
				_chargesCurrent = reader.ReadInt32();
				break;
			}
		}
	}
}