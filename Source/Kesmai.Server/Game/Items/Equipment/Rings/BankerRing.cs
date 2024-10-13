using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items;

public partial class BankerRing : Ring, ITreasure
{
	private static uint _carryLimit = 4000000000;
			
	private uint _consumed;
		
	/// <inheritdoc />
	public override uint BasePrice => 5000;

	/// <summary>
	/// Gets the gold held by this ring.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public uint Consumed
	{
		get => _consumed;
		set => _consumed = Math.Min(value, _carryLimit); /* The amount can never be greater than the limit. */
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BankerRing"/> class.
	/// </summary>
	public BankerRing() : base(502)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="BankerRing"/> class.
	/// </summary>
	public BankerRing(Serial serial) : base(serial)
	{
	}
		
	/* Feature 1: Dragging gold and dropping the ring allows for gold to be consumed and extracted. */

	/// <inheritdoc />
	/// <remarks>When the ring is dropped from the equipment container, we drop the gold held by the entity.</remarks>
	protected override bool OnUnequip(MobileEntity entity)
	{
		var segmentTile = entity.SegmentTile;

		if (segmentTile != null)
			PlaceGold(segmentTile);
			
		return base.OnUnequip(entity);
	}
		
	/// <inheritdoc />
	/// <remarks>
	/// We override the base logic which checks if the dropped item can stack.
	/// Any gold dropped onto the ring will be consumed, but only if equipped.
	/// </remarks>
	protected override bool OnReceiveDrop(MobileEntity entity, ItemEntity dropped)
	{
		var consumed = Consume(dropped);

		if (consumed)
			entity.SendLocalizedMessage(6200348); /* The ring holds the gold. */

		return consumed;
	}

	public bool Consume(ItemEntity dropped)
	{
		if (dropped is not Gold || Container is not Rings || dropped is null)
			return false; /* The item will bounce back to the original location. */

		Consumed += dropped.Amount;

		/* Since the item is deleted, the server will ignore further drop logic. */
		dropped.Amount = 1; /* Just in-case someone figures out any shenanigans. */
		dropped.Delete();
		return true;
	}

	/// <summary>
	/// Places the held gold to the specified tile.
	/// </summary>
	public void PlaceGold(SegmentTile segmentTile)
	{
		if (segmentTile is null || Consumed is 0)
			return;

		/* Move the created gold to the specified location. */
		var gold = new Gold()
		{
			Amount = Consumed
		};
		gold.Move(segmentTile.Location, true, segmentTile.Segment);

		/* Reset our gold counter. */
		Consumed = 0;
	}
		
	/* Feature 2: Double-clicking the ring while equipped allows a target to place the gold on an adjacent hex. */
	/// <inheritdoc />
	public override ActionType GetAction()
	{
		if (Container is Rings)
			return ActionType.Use;
			
		return base.GetAction();
	}
		
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Use)
			return base.HandleInteraction(entity, action);

		if (Container is not Rings)
			return false;

		if (Consumed > 0)
		{
			var segmentTile = entity.SegmentTile;
			var destinationTile = segmentTile;
			
			var component = default(TerrainComponent);
			
			(destinationTile, component) = entity.GetComponentInNeighbor<Counter>();
			
			if (component is null)
				(destinationTile, component) = entity.GetComponentInNeighbor<Altar>();

			if (destinationTile is null)
				destinationTile = segmentTile;

			/* Find the neighbor and place the gold from the ring. */
			PlaceGold(destinationTile);

			if (segmentTile != destinationTile)
				entity.SendLocalizedMessage(6200345); /* The gold is placed near to you. */
			else
				entity.SendLocalizedMessage(6200346); /* The gold is placed at your feet. */
				
			entity.QueueRoundTimer();
		}
		else
		{
			entity.SendLocalizedMessage(6200347); /* Target the gold you want to hold. */
			entity.Target = new InternalTarget(this);
		}
			
		return true;
	}
		
	private class InternalTarget : Target
	{
		private BankerRing _ring;
			
		public InternalTarget(BankerRing ring) : base(1, TargetFlags.Items)
		{
			_ring = ring;
		}
			
		/// <inheritdoc />
		protected override void OnTarget(MobileEntity source, object target)
		{
			if (!source.IsAlive || !source.CanPerformLift || target is not Gold gold) /* Can't finish the target if you died between. */
				return;

			if (!gold.OnDragLift(source))
				return;

			if (_ring.Consume(gold))
			{
				source.SendLocalizedMessage(6200348); /* The ring holds the gold. */
				source.QueueRoundTimer();
			}
		}
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200343)); /* [You are looking at] [a heavy gold ring with a faded inscription that reads "Pr..-rt. of .a-.er's G'.ld."] */

		if (Identified)
			entries.Add(new LocalizationEntry(6300424, Consumed.ToString())); /* The ring whispers: {0}. */
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
			
		writer.Write((uint)_consumed);
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
				_consumed = reader.ReadUInt32();
				break;
			}
		}

		/* Add some validation to prevent invalid amounts. */
		if (_consumed > _carryLimit)
			_consumed = _carryLimit;
	}
}