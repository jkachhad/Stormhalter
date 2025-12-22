using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Ring : ItemEntity
{
	/// <summary>
	/// Gets the label number.
	/// </summary>
	public override int LabelNumber => 6000073;

	/// <summary>
	/// Gets the item category.
	/// </summary>
	public override int Category => 4;

	/// <summary>
	/// Initializes a new instance of the <see cref="Ring"/> class.
	/// </summary>
	protected Ring(int ringID) : base(ringID)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Ring"/> class.
	/// </summary>
	protected Ring(Serial serial) : base(serial)
	{
	}
	
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (Container is Rings)
			entries.Add(UnequipRingInteraction.Instance);
		else
			entries.Add(EquipRingInteraction.Instance);
		
		entries.Add(InteractionSeparator.Instance);
		
		base.GetInteractions(source, entries);
	}

	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1);	/* version */
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

public class EquipRingInteraction : InteractionEntry
{
	public static readonly EquipRingInteraction Instance = new EquipRingInteraction();

	private EquipRingInteraction() : base("Equip", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Ring ring || ring.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		var rings = source.Rings;

		if (rings is null)
			return;

		var destinationSlot = rings.CheckHold(ring);

		if (!destinationSlot.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6300372); /* You do not have enough room to do that. */
			return;
		}

		source.Lift(ring, ring.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		source.DropHeld(InventoryGroup.Rings, destinationSlot.Value);
	}
}

public class UnequipRingInteraction : InteractionEntry
{
	public static readonly UnequipRingInteraction Instance = new UnequipRingInteraction();

	private UnequipRingInteraction() : base("Unequip")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Ring ring || ring.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		/* Prefer backpack, then belt, then a free hand. */
		var destinationGroup = default(InventoryGroup?);
		var destinationSlot = default(int?);

		var backpack = source.Backpack;

		if (backpack != null)
		{
			destinationSlot = backpack.CheckHold(ring);

			if (destinationSlot.HasValue)
				destinationGroup = InventoryGroup.Backpack;
		}

		if (!destinationGroup.HasValue)
		{
			if (source.HasFreeHand(out var handSlot) && handSlot.HasValue)
			{
				destinationGroup = InventoryGroup.Hands;
				destinationSlot = handSlot.Value;
			}
		}

		if (!destinationGroup.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6300372); /* You do not have enough room to do that. */
			return;
		}

		source.Lift(ring, ring.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		source.DropHeld(destinationGroup.Value, destinationSlot.Value);
	}
}
