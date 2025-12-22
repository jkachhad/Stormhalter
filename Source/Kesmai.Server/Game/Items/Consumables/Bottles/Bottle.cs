using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract class Bottle : Consumable
{
	public static double SpillChance = 0.25;

	private static Dictionary<int, int> _table = new()
	{
		/*	Closed,		Open	*/
		{ 6, 6 },
		{ 209, 92 },
		{ 210, 93 },
		{ 211, 41 },
		{ 212, 42 },
		{ 216, 16 },
		{ 222, 80 },
		{ 225, 83 },
		{ 227, 106 },
		{ 247, 94 },
		{ 274, 287 },
		{ 276, 275 },
		{ 289, 288 },
		{ 303, 79 },
		{ 316, 317 }
	};

	private bool _isOpen;
	
	/// <inheritdoc />
	public override int LabelNumber => 6000013;

	/// <inheritdoc />
	public override int Category => 12;
	
	/// <summary>
	/// Gets a value indicating if this bottle is un-corked.
	/// </summary>
	public bool IsOpen => _isOpen;

	public bool IsFull => _content != null;
	
	/// <inheritdoc />
	protected override bool IsConsumable(MobileEntity entity)
	{
		return IsFull;
	}

	public override int ItemId
	{
		get
		{
			var closedId = base.ItemId;

			if (_isOpen && _table.TryGetValue(closedId, out var openId))
				return openId;

			return closedId;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Bottle"/> class.
	/// </summary>
	public Bottle(int closedId) : base(closedId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Bottle"/> class.
	/// </summary>
	public Bottle(Serial serial) : base(serial)
	{
	}

	public override void GetDescription(List<LocalizationEntry> entries)
	{
		base.GetDescription(entries);

		if (IsOpen)
			entries.Add(new LocalizationEntry(6300383)); /* The container is open. */
	}
	
	public override void OnDropped()
	{
		base.OnDropped();

		/* Chance to spill when move around while open. */
		if (IsOpen && IsFull)
			if (Utility.RandomDouble() < SpillChance)
				Spill();
	}

	/// <summary>
	/// Spills the contents of the bottle.
	/// </summary>
	public void Spill()
	{
		if (!IsOpen || !IsFull)
			return;
		
		OnSpill();

		_content = null;
	}

	/// <summary>
	/// Called when the bottle spills its contents.
	/// </summary>
	/// <remarks>
	/// Spilled contents are accessible via the <see cref="Bottle._content" /> field.
	/// </remarks>
	protected virtual void OnSpill()
	{
	}

	/// <summary>
	/// Opens this bottle.
	/// </summary>
	public void Open(MobileEntity source)
	{
		if (IsOpen)
			return;

		/* Play a sound of the bottle un-corking */
		source.EmitSound(63, 3, 6);

		_isOpen = true;
		Delta(ItemDelta.UpdateIcon);
		
		OnOpen(source);
	}

	/// <summary>
	/// Called when the bottle is opened.
	/// </summary>
	protected virtual void OnOpen(MobileEntity source)
	{
	}

	/// <summary>
	/// Closes this bottle.
	/// </summary>
	public void Close(MobileEntity source)
	{
		if (!IsOpen)
			return;

		/* Play a sound of the bottle corking */
		source.EmitSound(10003, 3, 6);

		_isOpen = false;
		Delta(ItemDelta.UpdateIcon);
		
		OnClose(source);
	}

	/// <summary>
	/// Called when the bottle is closed.
	/// </summary>
	protected virtual void OnClose(MobileEntity source)
	{
	}
	
	/// <inheritdoc />
	protected override void OnConsume(MobileEntity entity, bool destroy = true)
	{
		/* Play a sound of un-corking and then delayed sound of drinking. */
		if (IsFull)
			entity.EmitSound(61, 3, 6);

		if (destroy)
			entity.EmitSound(71, 3, 6, IsFull ? TimeSpan.FromSeconds(1.9f) : TimeSpan.FromSeconds(0.4f));

		/* Call the base method to execute the content effect. */
		base.OnConsume(entity, destroy);
	}

	/// <inheritdoc />
	public override bool ThrowAt(MobileEntity source, MobileEntity entity)
	{
		/* Play a sound of a bottle breaking at the target location. */
		Segment.PlaySound(entity.Location, 71, 3, 6);

		/* Delete the item as it's now broken. */
		Delete();

		/* Call the base method to execute the content effect. */
		return base.ThrowAt(source, entity);
	}

	/// <inheritdoc />
	public override bool ThrowAt(MobileEntity source, Point2D location)
	{
		/* Play a sound of a bottle breaking at the target location. */
		Segment.PlaySound(location, 71, 3, 6);

		/* Delete the item as it's now broken. */
		Delete();

		/* Call the base method to execute the content effect. */
		return base.ThrowAt(source, location);
	}
	
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		base.GetInteractions(source, entries);

		entries.Add(InteractionSeparator.Instance);
		
		entries.Add(DrinkBottleInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		if (!IsOpen)
			entries.Add(OpenBottleInteraction.Instance);
		else
			entries.Add(CloseBottleInteraction.Instance);
	}
	
	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)2); /* version */

		writer.Write(_isOpen);
	}

	/// <inheritdoc />
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 2:
			{
				_isOpen = reader.ReadBoolean();

				goto case 1;
			}
			case 1:
			{
				break;
			}
		}
	}

	internal bool CanToggle(PlayerEntity source, bool opening, bool sendTranceMessage = false)
	{
		if (source is null || source.Deleted || Deleted)
			return false;

		if (!source.CanPerformAction)
			return false;

		if (source.Tranced && !source.IsSteering)
		{
			if (sendTranceMessage)
				source.SendLocalizedMessage(System.Drawing.Color.Red, 6300200); /* You can't do that while in a trance. */
			return false;
		}

		var owner = source.IsSteering ? source.Steering : source;

		if (owner is null || Parent != owner)
			return false;

		if (Container is not Hands)
			return false;

		return opening ? !IsOpen : IsOpen;
	}
}

/// <summary>
/// Interaction entry for drinking from a bottle.
/// </summary>
/// <remarks>
/// This interaction allows a player to drink from a bottle if it is open and contains consumable content.
/// </remarks>
public class DrinkBottleInteraction : InteractionEntry
{
	public static readonly DrinkBottleInteraction Instance = new DrinkBottleInteraction();
	
	private DrinkBottleInteraction() : base("Drink")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (target is not Bottle bottle)
			return;

		if (bottle.Consume(source, !bottle.IsOpen))
			source.QueueRoundTimer();
	}

	public override bool CanExecute(PlayerEntity source, WorldEntity target)
	{
		if (!base.CanExecute(source, target))
			return false;
		
		if (target is not Bottle bottle)
			return false;

		return bottle.CanConsume(source);
	}
}

/// <summary>
/// Interaction entry for opening a bottle.
/// </summary>
/// <remarks>
/// This interaction allows a player to open a bottle if it is currently closed.
/// </remarks>
public class OpenBottleInteraction : InteractionEntry
{
	public static readonly OpenBottleInteraction Instance = new OpenBottleInteraction();
	
	private OpenBottleInteraction() : base("Open")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (target is not Bottle bottle)
			return;

		if (!bottle.CanToggle(source, opening: true, sendTranceMessage: true))
			return;

		bottle.Open(source);
		
		source.QueueRoundTimer();
	}

	public override bool CanExecute(PlayerEntity source, WorldEntity target)
	{
		if (!base.CanExecute(source, target))
			return false;
		
		if (target is not Bottle bottle)
			return false;
		
		return bottle.CanToggle(source, opening: true);
	}
}

/// <summary>
/// Interaction entry for closing a bottle.
/// </summary>
/// <remarks>
/// This interaction allows a player to close a bottle if it is currently open.
/// </remarks>
public class CloseBottleInteraction : InteractionEntry
{
	public static readonly CloseBottleInteraction Instance = new CloseBottleInteraction();
	
	private CloseBottleInteraction() : base("Close")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (target is not Bottle bottle)
			return;

		if (!bottle.CanToggle(source, opening: false, sendTranceMessage: true))
			return;

		bottle.Close(source);
		
		source.QueueRoundTimer();
	}

	public override bool CanExecute(PlayerEntity source, WorldEntity target)
	{
		if (!base.CanExecute(source, target))
			return false;
		
		if (target is not Bottle bottle)
			return false;
		
		return bottle.CanToggle(source, opening: false);
	}
}
