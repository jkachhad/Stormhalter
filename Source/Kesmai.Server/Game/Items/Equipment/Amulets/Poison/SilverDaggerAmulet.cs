using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public class SilverDaggerAmulet : Amulet, ITreasure, ICharged
{
	private int _chargesCurrent;
	private int _chargesMax;

	[CommandProperty(AccessLevel.GameMaster)]
	public int ChargesCurrent
	{
		get => _chargesCurrent;
		set
		{
			var newValue = value.Clamp(0, _chargesMax);

			if (_chargesCurrent != newValue)
			{
				_chargesCurrent = newValue;
				InvalidateTooltip();
			}
		}
	}
		
	[CommandProperty(AccessLevel.GameMaster)]
	public int ChargesMax
	{
		get => _chargesMax;
		set
		{
			if (_chargesMax != value)
			{
				_chargesMax = value;
				InvalidateTooltip();
			}
		}
	}
		
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 1000;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 100;

	/// <inheritdoc />
	public override bool RequiresIdentification => true;

	public SilverDaggerAmulet() : this(20)
	{
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="SilverDaggerAmulet"/> class.
	/// </summary>
	public SilverDaggerAmulet(int charges = 20) : base(304)
	{
		_chargesCurrent = charges;
		_chargesMax = charges;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="SilverDaggerAmulet"/> class.
	/// </summary>
	public SilverDaggerAmulet(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override IEnumerable<LocalizationEntry> AddDescriptionProperty(EntityTooltipPacket tooltip, PlayerEntity beholder)
	{
		yield return LocalizationEntry.Get(6200072); /* [a small silver dagger with an emerald blade hanging from a silver chain.] */

		if (Identified)
			yield return LocalizationEntry.Get(6250051); /* The amulet contains the spell of Protection from Poison. */
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be applied to <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnActivateBonus(MobileEntity entity)
	{
		base.OnActivateBonus(entity);

		if (_chargesCurrent > 0)
		{
			if (!entity.GetStatus(typeof(PoisonProtectionStatus), out var status))
			{
				status = new PoisonProtectionStatus(entity)
				{
					Inscription = new SpellInscription { SpellId = 84 }
				};
				status.AddSource(new ItemSource(this));

				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}
		}
	}

	/// <summary>
	/// Overridable. Called when effects from this item should be removed from <see cref="MobileEntity"/>.
	/// </summary>
	protected override void OnInactivateBonus(MobileEntity entity)
	{
		base.OnInactivateBonus(entity);

		if (entity.GetStatus(typeof(PoisonProtectionStatus), out var status))
			status.RemoveSource(this);
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
