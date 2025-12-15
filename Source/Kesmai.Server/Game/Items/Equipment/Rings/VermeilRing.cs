using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items;

public class VermeilRing : Ring, ITreasure
{
	/// <summary>
	/// Gets the price.
	/// </summary>
	public override uint BasePrice => 300;

	/// <summary>
	/// Gets the weight.
	/// </summary>
	public override int Weight => 20;

	/// <summary>
	/// Initializes a new instance of the <see cref="VermeilRing"/> class.
	/// </summary>
	public VermeilRing() : base(218)
	{
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="VermeilRing"/> class.
	/// </summary>
	public VermeilRing(Serial serial) : base(serial)
	{
	}

	/// <summary>
	/// Gets the description for this instance.
	/// </summary>
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200007)); /* [You are looking at] [a vermeil ring.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6250006)); /* The ring contains the spell of Identify. */
	}

	/// <inheritdoc />
	public override ActionType GetAction()
	{
		var parent = Parent;

		if (parent is MobileEntity mobile && mobile.LeftHand == this)
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
			
		if (entity is PlayerEntity player)
		{
			player.EmitSound(237, 3, 6);

			if (player.RightHand != null)
			{
				player.RightHand.Identified = true;
				player.SendDescription(player.RightHand);
			}

			player.QueueRoundTimer();
		}

		return true;
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
