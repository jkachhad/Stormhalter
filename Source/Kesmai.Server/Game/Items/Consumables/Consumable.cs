using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Miscellaneous;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

[WorldForge]
public abstract class Consumable : ItemEntity
{
	protected IConsumableContent _content;
		
	/// <inheritdoc />
	public override int Category => 12;

	/// <summary>
	/// Gets or sets the <see cref="IConsumableContent"/> for this <see cref="Consumable"/>.
	/// </summary>
	[WorldForge]
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
		writer.Write((bool)(_content != null));

		if (_content != null)
		{
			var contentType = _content.GetType();
			var contentTypeName = contentType.FullName;

			if (!String.IsNullOrEmpty(contentTypeName))
			{
				writer.Write((string)contentTypeName);
					
				/* Store starting and ending location of the stream. */
				var start = (int)writer.WrittenCount;
				_content.Serialize(writer);
				var end = (int)writer.WrittenCount;

				/* Write the number of bytes serialized and shift the data. */
				writer.Insert(start, BitConverter.GetBytes(end - start));
			}
			else
			{
				writer.Write((string)String.Empty);
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
	/// Called when the specified entity consumes this item.
	/// </summary>
	[WorldForge]
	public bool Consume(MobileEntity entity, bool destroy = true)
	{
		if (Deleted || entity.Deleted)
			return false;

		if (HeldBy.Count > 0)
			OnDropped();
			
		OnConsume(entity, destroy);

		_content = null;

		if (destroy)
			Delete();

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