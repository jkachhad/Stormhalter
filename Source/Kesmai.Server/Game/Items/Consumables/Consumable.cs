using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract class Consumable : ItemEntity
{
	protected IConsumableContent _content;
		
	/// <inheritdoc />
	public override int Category => 12;

	/// <summary>
	/// Gets or sets the <see cref="IConsumableContent"/> for this <see cref="Consumable"/>.
	/// </summary>
	public IConsumableContent Content
	{
		get => _content;
		set => _content = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Consumable"/> class.
	/// </summary>
	protected Consumable(int consumableID) : base(consumableID)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Consumable"/> class.
	/// </summary>
	protected Consumable(Serial serial) : base(serial)
	{
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)2); /* version */

		// write a flag indicating if we have contents.
		writer.Write(_content != null);

		if (_content != null)
		{
			var contentType = _content.GetType();
			var contentTypeName = contentType.FullName;

			if (!String.IsNullOrEmpty(contentTypeName))
			{
				writer.Write(contentTypeName);
					
				/* Store starting and ending location of the stream. */
				var start = writer.WrittenCount;
				_content.Serialize(writer);
				var end = writer.WrittenCount;

				/* Write the number of bytes serialized and shift the data. */
				writer.Insert(start, BitConverter.GetBytes(end - start));
			}
			else
			{
				writer.Write(String.Empty);
			}
		}
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
				var hasContents = reader.ReadBoolean();
				var content = default(IConsumableContent);
					
				if (hasContents)
				{
					var contentTypeName = reader.ReadString();

					if (!String.IsNullOrEmpty(contentTypeName))
					{
						var contentType = TypeCache.FindTypeByFullName(contentTypeName);
						var writeLength = reader.ReadInt32();
						var writeEnd = reader.ConsumedCount + writeLength;

						if (contentType != null && Activator.CreateInstance(contentType) is IConsumableContent serialized)
						{
							try
							{
								/* Attempt to do a read of the serialized content. */
								var start = reader.ConsumedCount;
								serialized.Deserialize(ref reader);
								var end = reader.ConsumedCount;
									
								var readLength = end - start;

								/* The read length and write didn't match. */
								if (readLength != writeLength)
									throw new ArgumentOutOfRangeException(nameof(readLength));
									
								content = serialized;
							}
							catch
							{
								/* Move to the end of the content write. */
								reader.Advance(writeEnd - reader.ConsumedCount);
							}
						}
						else
						{
							/* If the type could not be created, move the length of the content data. */
							reader.Advance(writeEnd - reader.ConsumedCount);
						}
					}
				}

				/* Replace the content with serialized content, or null if none. */
				_content = content;
				break;
			}
		}
	}
		
	/// <summary>
	/// Determines whether this instance may be consumed by the specified entity.
	/// </summary>
	/// <remarks>
	/// This method performs validation checks to determine if the specified entity
	/// may consume this item. It checks if the entity is alive, if the item is
	/// consumable by the entity, and if the entity can lift and carry the item.
	/// </remarks>
	public bool CanConsume(MobileEntity entity, bool requireLiftChecks = true)
	{
		// Validate if the entity can consume this item without performing lift actions.
		return ValidateConsume(entity, requireLiftChecks, performLiftActions: false);
	}

	/// <summary>
	/// Overridable. Determines whether this instance is consumable.
	/// </summary>
	/// <remarks>
	/// Override this method to provide custom logic to determine if the specified entity
	/// may consume this item. For example, a potion that can only be consumed by a specific
	/// class or alignment. The default implementation always returns true.
	/// </remarks>
	protected virtual bool IsConsumable(MobileEntity entity)
	{
		// By default, all consumables are consumable.
		return true;
	}

	/// <summary>
	/// Called when the specified entity consumes this item.
	/// </summary>
	/// <remarks>
	/// This method performs validation checks and, if successful, triggers the consumption
	/// process. It checks if the entity is alive, if the item is consumable by the entity,
	/// and if the entity can lift and carry the item. If the item is held by any entities,
	/// it is dropped before consumption. The <see cref="OnConsume"/> method is then called to handle
	/// the actual consumption logic. If the <paramref name="destroy"/> parameter is true, the item is deleted
	/// after consumption.
	/// </remarks>
	public bool Consume(MobileEntity entity, bool destroy = true)
	{
		// Validate if the entity can consume this item.
		if (!ValidateConsume(entity, requireLiftChecks: true, performLiftActions: true))
			return false;

		// If the item is held by any entities, drop it first.
		if (HeldBy.Count > 0)
			OnDropped();

		// Trigger the consumption process.
		OnConsume(entity, destroy);

		_content = null;

		// Delete the item if specified.
		if (destroy)
			Delete();

		// Consumption successful.
		return true;
	}

	private bool ValidateConsume(MobileEntity entity, bool requireLiftChecks, bool performLiftActions)
	{
		if (Deleted || entity is null || entity.Deleted)
			return false;

		// Check if the entity is alive. Dead entities cannot consume items.
		if (!entity.IsAlive)
			return false;

		// Check if this item is consumable by the entity. The consumable may be in a state
		// that prevents it from being consumed, such as being empty or spoiled.
		if (!IsConsumable(entity))
			return false;

		// Check if the entity can lift and carry this item. If the item is already held by the entity,
		// we skip the lift checks to avoid redundant validation.
		var heldByEntity = ReferenceEquals(entity.Holding, this) && HeldBy.Contains(entity);

		if (entity.Holding != null && !heldByEntity)
			return false;

		var applyLiftChecks = requireLiftChecks && !heldByEntity;

		// Perform lift checks if required.
		if (!applyLiftChecks)
			return true;
		
		// Check if the entity can perform lift actions. If not, consumption is not possible.
		if (!entity.CanPerformLift || !entity.CheckLift(entity, this))
			return false;
		
		// Check for non-local lift restrictions from the parent container.
		if (Parent is MobileEntity parent && !parent.CheckNonlocalLift(entity, this))
			return false;
		
		// Notify both the entity and the item about the lift action.
		if (performLiftActions)
		{
			if (!entity.OnDragLift(this))
				return false;

			if (!OnDragLift(entity))
				return false;
		}

		// Check if the container can have this item removed.
		var container = Container;

		if (container is null || !container.CanRemove(this))
			return false;

		// Finally, check if the entity can carry the additional weight of this item.
		if (!entity.CanCarry(this, Amount))
		{
			if (performLiftActions)
				entity.SendLocalizedMessage(6300320); /* You are not able to carry that much more weight. */
			
			return false;
		}

		// All checks passed; the item can be consumed by the entity.
		return true;
	}
		
	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		if (_content is not null)
			_content.GetDescription(this, entries);
	}

	/// <summary>
	/// Called when this instance is consumed by the specified entity.
	/// </summary>
	/// <remarks>
	/// Override this method to execute an action before or after the <see cref="IConsumableContent"/>
	/// is triggered. If the base method is not called, the <see cref="IConsumableContent"/> may not occur.
	/// </remarks>
	protected virtual void OnConsume(MobileEntity entity, bool destroy = true)
	{
		if (_content is not null)
			_content.OnConsume(entity, this);
	}

	/// <inheritdoc />
	public override bool ThrowAt(MobileEntity source, MobileEntity entity)
	{
		/* We override the default behavior of just using the target's location. We may have a
		 * consumable just affect the targeted entity. */
		if (_content is not null)
			_content.ThrowAt(source, this, entity);

		return true;
	}

	/// <inheritdoc />
	public override bool ThrowAt(MobileEntity source, Point2D location)
	{
		if (_content is not null)
			_content.ThrowAt(source, this, location);
			
		return true;
	}
}
