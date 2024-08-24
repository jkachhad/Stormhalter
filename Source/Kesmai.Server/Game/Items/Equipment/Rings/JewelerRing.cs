using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items;

public partial class JewelerRing : Ring, ITreasure
{
	public static int TransmuteDelayRounds = 60;
		
	/// <inheritdoc />
	public override uint BasePrice => 2000;

	private ulong _transmuted;
	private Timer _cooldownTimer;
		
	/// <summary>
	/// Gets the total gold value converted by this ring.
	/// </summary>
	[WorldForge]
	[CommandProperty(AccessLevel.GameMaster)]
	public ulong Transmuted
	{
		get => _transmuted;
		set => _transmuted = value; /* The amount has been transmuted. */
	}

	public bool CanTransmute => _cooldownTimer is null;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="JewelerRing"/> class.
	/// </summary>
	public JewelerRing() : base(503)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="JewelerRing"/> class.
	/// </summary>
	public JewelerRing(Serial serial) : base(serial)
	{
	}

	protected override bool OnEquip(MobileEntity entity)
	{
		var delay = entity.Facet.TimeSpan.FromRounds(TransmuteDelayRounds);
			
		if (delay > TimeSpan.Zero)
			StartCooldown(delay);

		entity.SendLocalizedMessage(6200349); /* The ring's power fades and begins to recharge. */
			
		return base.OnEquip(entity);
	}

	private void StartCooldown(TimeSpan delay)
	{
		_cooldownTimer = Timer.DelayCall(delay, ClearCooldown); /* Delay use by TransmuteDelayRounds. */

		Hue = Color.Gray;
			
		Delta(ItemDelta.UpdateAction);
	}

	private void ClearCooldown()
	{
		if (_cooldownTimer != null)
		{
			_cooldownTimer.Stop();
			_cooldownTimer = null;
		}

		Hue = Color.Transparent;

		Delta(ItemDelta.UpdateAction);
	}

	/* Feature 1: Double-clicking the ring while equipped allows a target to convert gem into gold. */
	/// <inheritdoc />
	public override ActionType GetAction()
	{
		if (Container is Rings && CanTransmute)
			return ActionType.Use;
			
		return base.GetAction();
	}
		
	public override bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Use)
			return base.HandleInteraction(entity, action);

		if (Container is not Rings || !CanTransmute)
			return false;
			
		entity.SendLocalizedMessage(6200350); /* Target a gem to transmute. */
		entity.Target = new InternalTarget(this);
		return true;
	}
		
	private class InternalTarget : Target
	{
		private JewelerRing _ring;

		public InternalTarget(JewelerRing ring) : base(1, TargetFlags.Items)
		{
			_ring = ring;
		}

		protected override void OnTarget(MobileEntity source, object target)
		{
			if (!source.IsAlive || target is not Gem gem || gem.ActualPrice is 0)
				return;

			var backpack = source.Backpack;

			if (backpack is null)
				return;

			var gold = new Gold()
			{
				Amount = gem.ActualPrice,
			};
				
			var slot = default(int?);
				
			var goldInBackpack = backpack.FirstOrDefault((i) => i is Gold);

			if (goldInBackpack != null)
				slot = goldInBackpack.Slot;
				
			if (!slot.HasValue)
				slot = backpack.CheckHold(gold);

			if (slot.HasValue)
				gold.DropToContainer(backpack, slot.Value);
			else
				gold.Move(source.Location, true, source.Segment);

			source.SendLocalizedMessage(slot.HasValue ? 
				6200351 : 6200352); /* The gem turns to gold in your backpack. */ /* The gem turns to gold at your feet. */
			source.QueueRoundTimer();
				
			gem.Delete();

			_ring.Transmuted += gem.ActualPrice;
				
			var delay = source.Facet.TimeSpan.FromRounds(TransmuteDelayRounds);
			
			if (delay > TimeSpan.Zero)
				_ring.StartCooldown(delay);
				
			source.SendLocalizedMessage(6200349); /* The ring's power fades and begins to recharge. */
		}
	}

	/// <inheritdoc />
	public override void GetDescription(List<LocalizationEntry> entries)
	{
		entries.Add(new LocalizationEntry(6200000, 6200344)); /* [You are looking at] [a light gold ring with a red ruby. Peering into the gem, you see it transmutate to pure gold.] */

		if (Identified)
			entries.Add(new LocalizationEntry(6300424, Transmuted.ToString())); /* The ring whispers: {0}. */
	}

	/// <inheritdoc />
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
			
		writer.Write((ulong)_transmuted);
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
				_transmuted = reader.ReadUInt64();
				break;
			}
		}
	}
}