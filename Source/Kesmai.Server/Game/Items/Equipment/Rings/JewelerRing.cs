using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public partial class JewelerRing : Ring, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		private ulong _transmuted;
		
		/// <summary>
		/// Gets the gold value converted by this ring.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public ulong Transmuted
		{
			get => _transmuted;
			set => _transmuted = value; /* The amount has been transmuted. */
		}
		
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
		
		/* Feature 1: Double-clicking the ring while equipped allows a target to convert gem into gold. */
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
				
				gem.Delete();

				_ring.Transmuted += gem.ActualPrice;
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
		public override void Serialize(BinaryWriter writer)
		{
			base.Serialize(writer);

			writer.Write((short)1); /* version */
			
			writer.Write((ulong)_transmuted);
		}

		/// <inheritdoc />
		public override void Deserialize(BinaryReader reader)
		{
			base.Deserialize(reader);

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
}