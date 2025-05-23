using System;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items;

public abstract class LocateAmulet : Amulet, ITreasure, ICharged
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
	/// Initializes a new instance of the <see cref="LocateAmulet"/> class.
	/// </summary>
	protected LocateAmulet(int amuletId, int charges = 3) : base(amuletId)
	{
		_chargesCurrent = charges;
		_chargesMax = charges;
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LocateAmulet"/> class.
	/// </summary>
	protected LocateAmulet(Serial serial) : base(serial)
	{
	}
		
	/// <inheritdoc />
	public override ActionType GetAction()
	{
		var container = Container;
			
		if (container is Hands || container is Paperdoll) 
			return ActionType.Use;

		return base.GetAction();
	}

	/// <inheritdoc />
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Use)
			return base.HandleInteraction(entity, action);
			
		if (!entity.CanPerformAction)
			return false;
			
		if (_chargesCurrent > 0)
		{
			var spell = new InstantLocateSpell
			{
				Item = this,
				Cost = 0,
			};

			spell.Warm(entity);
			spell.Cast();
		}

		return true;
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