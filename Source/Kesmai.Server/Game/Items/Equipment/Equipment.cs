using System.Collections.Generic;
using System.Drawing;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Equipment : ItemEntity
{
	/// <summary>
	/// Gets the hindrance penalty for this <see cref="Equipment"/>.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int Hindrance => 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromDaze => 0;

	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromConcussion => 0;
		
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromFire => 0;
		
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromIce => 0;
		
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ProtectionFromConcussion => 0;
		
	/// <summary>
	/// Gets the health regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int HealthRegeneration => 0;
		
	/// <summary>
	/// Gets the stamina regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int StaminaRegeneration => 0;
		
	/// <summary>
	/// Gets the mana regeneration provided by this <see cref="Equipment"/>
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual int ManaRegeneration => 0;

	/// <summary>
	/// Gets a value indicating if this instance restricts spell casting for certain professions.
	/// </summary>
	[CommandProperty(AccessLevel.GameMaster)]
	public virtual bool RestrictSpellcast => false;

	/// <summary>
	/// Initializes a new instance of the <see cref="Equipment"/> class.
	/// </summary>
	/// <remarks>
	/// Using the default constructor should be avoided as it may
	/// result in an uninitialized instance. This constructor is primarily
	/// provided to facilitate deserialization processes.
	/// </remarks>
	public Equipment()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Equipment"/> class.
	/// </summary>
	protected Equipment(int equipmentId) : base(equipmentId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Equipment"/> class.
	/// </summary>
	protected Equipment(Serial serial) : base(serial)
	{
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
	/// Deserialize this instance from persisted binary data.
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

	/// <inheritdoc />
	protected override PassiveBonusSet CreatePassiveBonuses(MobileEntity wearer)
	{
		var bonuses = base.CreatePassiveBonuses(wearer);

		if (CanApplyPassiveBonuses(wearer))
		{
			if (ProtectionFromFire > 0)
				bonuses.Add(EntityStat.FireProtection, ProtectionFromFire);
				
			if (ProtectionFromIce > 0)
				bonuses.Add(EntityStat.IceProtection, ProtectionFromIce);
				
			if (ProtectionFromDaze > 0)
				bonuses.Add(EntityStat.DazeProtection, ProtectionFromDaze);

			if (HealthRegeneration > 0)
				bonuses.Add(EntityStat.HealthRegenerationRate, HealthRegeneration);

			if (StaminaRegeneration > 0)
				bonuses.Add(EntityStat.StaminaRegenerationRate, StaminaRegeneration);

			if (ManaRegeneration > 0)
				bonuses.Add(EntityStat.ManaRegenerationRate, ManaRegeneration);
		}

		return bonuses;
	}
	
	/// <inheritdoc />
	protected override void OnBreak(MobileEntity source)
	{
		base.OnBreak(source);

		if (this is not ITreasure)
			return;

		// Only players get experience when breaking items.
		if (source is not PlayerEntity player)
			return;

		var value = 0;

		// Conjured (recall rings) and purchased items do not provide a bonus.
		if (IsConjured || IsPurchased)
			return;

		var multiplier = 10;

		if (IsBound)
			multiplier += 5;

		value = Utility.RandomRange((int)(ActualPrice * multiplier), 0.5, 0.9);

		if (Owner != null && Owner != player)
		{
			if (player.Alignment.IsAny(Alignment.Lawful))
				player.Alignment = Alignment.Neutral;
		}
				
		if (value > 0)
			player.AwardExperience(value);
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (Container is Paperdoll)
			entries.Add(UnequipPaperdollInteraction.Instance);
		else
			entries.Add(EquipPaperdollInteraction.Instance);
		
		entries.Add(InteractionSeparator.Instance);
		
		base.GetInteractions(source, entries);
	}
}

public class EquipPaperdollInteraction : InteractionEntry
{
	public static readonly EquipPaperdollInteraction Instance = new EquipPaperdollInteraction();

	private EquipPaperdollInteraction() : base("Equip", range: 0)
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Equipment equipment || equipment.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		var container = source.Paperdoll;

		if (container is null)
			return;

		var destinationSlot = container.CheckHold(equipment);

		if (!destinationSlot.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6300372); /* You do not have enough room to do that. */
			return;
		}

		source.Lift(equipment, equipment.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		source.DropHeld(InventoryGroup.Portrait, destinationSlot.Value);
	}
}

public class UnequipPaperdollInteraction : InteractionEntry
{
	public static readonly UnequipPaperdollInteraction Instance = new UnequipPaperdollInteraction();
	
	private UnequipPaperdollInteraction() : base("Unequip")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not Equipment equipment || equipment.Deleted)
			return;

		if (!source.CanPerformAction)
			return;

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		/* Determine the best destination for the unequipped item. Prefer backpack,
		 * then belt, and finally a free hand. */
		var destinationGroup = default(InventoryGroup?);
		var destinationSlot = default(int?);

		var backpack = source.Backpack;

		if (backpack != null)
		{
			destinationSlot = backpack.CheckHold(equipment);

			if (destinationSlot.HasValue)
				destinationGroup = InventoryGroup.Backpack;
		}

		if (!destinationGroup.HasValue)
		{
			var belt = source.Belt;

			if (belt != null)
			{
				destinationSlot = belt.CheckHold(equipment);

				if (destinationSlot.HasValue)
					destinationGroup = InventoryGroup.Belt;
			}
		}

		if (!destinationGroup.HasValue)
		{
			if (source.HasFreeHand(out var handSlot) && handSlot.HasValue)
			{
				destinationGroup = InventoryGroup.Hands;
				destinationSlot = handSlot.Value;
			}
		}

		// If we couldn't find a destination, inform the user and exit.
		if (!destinationGroup.HasValue)
		{
			source.SendLocalizedMessage(Color.Red, 6300372); /* You do not have enough room to do that. */
			return;
		}

		// Attempt to lift the item first.
		source.Lift(equipment, equipment.Amount, out var liftRejectReason);

		if (liftRejectReason.HasValue)
			return;

		// Drop the item into the destination.
		source.DropHeld(destinationGroup.Value, destinationSlot.Value);
	}
}
