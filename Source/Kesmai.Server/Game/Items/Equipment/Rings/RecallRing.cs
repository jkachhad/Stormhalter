using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class RecallRing : Ring
	{
		private bool _isActive;
		private int _power;
		
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 150;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;
		
		/// <summary>
		/// Gets or sets the segment bound to this ring.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public Segment BoundSegment { get; set; }
		
		/// <summary>
		/// Gets or sets the location bound to this ring.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public Point2D BoundLocation { get; set; }
		
		/// <summary>
		/// Gets a value indicating whether this ring is active.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsActive => _isActive;

		/// <summary>
		/// Gets the recall power for this ring.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Power => _power;

		/// <summary>
		/// Initializes a new instance of the <see cref="RecallRing"/> class.
		/// </summary>
		public RecallRing() : this(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RecallRing"/> class.
		/// </summary>
		public RecallRing(int power) : base((power > 1 ? 393 : 246))
		{
			_power = power;
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200030)); /* [You are looking at] [a small gold ring emitting a faint blue glow.] */
		}

		/// <summary>
		/// Determines whether this instance can be equipped.
		/// </summary>
		public override bool CanEquip(MobileEntity entity)
		{
			/* The presence of high levels of the Dark Power prevents you from removing the ring. */
			var segment = entity.Segment;
			var location = entity.Location;

			var subregion = segment.GetSubregion(location);

			if (subregion != null && !subregion.AllowRecall)
			{
				entity.SendLocalizedMessage(6300353); /* The presence of high levels of the Dark Power prevents you from putting on the ring. */
				return false;
			}

			var worldTile = segment.FindTile(location);

			if (!worldTile.CanEnter(entity))
			{	
				entity.SendLocalizedMessage(Color.Red, 6300062); /* You may not put on that ring here. */
				return false;
			}

			return base.CanEquip(entity);
		}

		/// <summary>
		/// Determines whether this instance can be unequipped.
		/// </summary>
		public override bool CanUnequip(MobileEntity entity)
		{
			var segment = entity.Segment;
			var location = entity.Location;
			
			var subregion = segment.GetSubregion(location);

			if (subregion != null && !subregion.AllowRecall)
			{
				entity.SendLocalizedMessage(6300354); /* The presence of high levels of the Dark Power prevents you from removing the ring. */
				return false;
			}

			return base.CanUnequip(entity);
		}

		/// <summary>
		/// Called when this <see cref="ItemEntity" /> is equipped by the specified <see cref="MobileEntity" />
		/// </summary>
		protected override bool OnEquip(MobileEntity entity)
		{
			/* Recalls rings trigger their effect when equipped. When the world is loaded, each item is
			 * called to be equipped on the respective entity. Recall rings that are not active and
			 * in a container slot, would inappropriately trigger to be equipped. */
			if (PersistenceManager.IsLoading)
				return false;
			
			if (_isActive || !base.OnEquip(entity))
				return false;

			BoundSegment = entity.Segment;
			BoundLocation = entity.Location;

			entity.SendLocalizedMessage(6300061); /* You feel a tingling sensation.*/

			_isActive = true;

			return true;
		}

		/// <summary>
		/// Called when this <see cref="ItemEntity" /> is unquipped by the specified <see cref="MobileEntity" />
		/// </summary>
		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (_isActive)
			{
				if (!entity.Deleted && entity.IsAlive) /* Only teleport the entity if they are alive. */
				{
					var sourceSegment = entity.Segment;
					var sourceLocation = entity.Location;

					var targetSegment = BoundSegment;
					var targetLocation = BoundLocation;

					var sourceSound = targetLocation.GetDistanceToMax(sourceLocation) > 3;

					if (targetSegment != default(Segment))
						targetSegment.PlaySound(targetLocation, 66, 3, 6);

					if (sourceSound)
						sourceSegment.PlaySound(sourceLocation, 66, 3, 6);

					entity.Teleport(targetLocation, targetSegment);
				}

				_isActive = false;

				Delete();
			}

			return true;
		}

		/// <summary>
		/// Resets this instance.
		/// </summary>
		public void Reset()
		{
			BoundSegment = default(Segment);
			BoundLocation = default(Point2D);

			_isActive = false;
		}
	}
}
