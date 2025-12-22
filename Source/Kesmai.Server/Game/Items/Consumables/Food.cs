using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items;

public abstract class Food : Consumable
{
	/// <summary>
	/// Initializes a new instance of the <see cref="Food"/> class.
	/// </summary>
	public Food(int foodId) : base(foodId)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Food"/> class.
	/// </summary>
	public Food(Serial serial) : base(serial)
	{
	}
	
	/// <inheritdoc />
	protected override void OnConsume(MobileEntity entity, bool destroy = true)
	{
		/* Play a sound of chewing. */
		entity.EmitSound(62, 3, 6);

		/* Call the base method to execute the content effect. */
		base.OnConsume(entity, destroy);
	}

	/// <inheritdoc />
	protected override bool IsConsumable(MobileEntity entity)
	{
		return _content != null;
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		base.GetInteractions(source, entries);

		entries.Add(EatFoodInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);
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

/// <summary>
/// Interaction for consuming food.
/// </summary>
/// <remarks>
/// This interaction allows a player to eat food items in the game.
/// When executed, it checks if the target entity is a food item and
/// attempts to consume it. If the consumption is successful, it queues
/// a round timer for the player.
/// </remarks>
public class EatFoodInteraction : InteractionEntry
{
	public static readonly EatFoodInteraction Instance = new EatFoodInteraction();

	private EatFoodInteraction() : base("Eat")
	{
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (target is not Food food)
			return;
		
		if (food.Consume(source))
			source.QueueRoundTimer();
	}

	public override bool CanExecute(PlayerEntity source, WorldEntity target)
	{
		if (!base.CanExecute(source, target))
			return false;
		
		if (target is not Food food)
			return false;

		return food.CanConsume(source);
	}
}
