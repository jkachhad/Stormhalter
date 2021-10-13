using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Items
{
	public abstract partial class SpellOrb : ItemEntity
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000006;

		/// <inheritdoc />
		public override int Weight => 300;

		/// <inheritdoc />
		public override int Category => 3;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SpellOrb"/> class.
		/// </summary>
		protected SpellOrb(int orbId) : base(orbId)
		{
		}

		/// <inheritdoc />
		public override ActionType GetAction()
		{
			/* This item can be thrown from either the hands, or top 5-slots of the backpack. */
			var container = Container;

			if ((container is Hands) || (container is Backpack && container.GetSlot(this) < 5))
				return ActionType.Throw;
			
			return base.GetAction();
		}
		
		/// <inheritdoc />
		public override bool HandleInteraction(MobileEntity entity, ActionType action)
		{
			if (action != ActionType.Throw)
				return base.HandleInteraction(entity, action);
	
			entity.Target = new InternalTarget(this);
			return true;
		}
		
		public override bool ThrowAt(MobileEntity source, Point2D location)
		{
			var segment = source.Segment;
			var tile = segment.FindTile(location);

			if (tile != null && tile.AllowsSpellPath())
				PlaceEffect(source, location);
			
			Delete();
			return true;
		}

		protected abstract void PlaceEffect(MobileEntity source, Point2D location);

		private class InternalTarget : Target
		{
			private SpellOrb _orb;
			
			public InternalTarget(SpellOrb orb) : base(4, TargetFlags.Path | TargetFlags.Harmful)
			{
				_orb = orb;
			}
			
			protected override void OnTarget(MobileEntity source, object target)
			{
				if (target is MobileEntity mobile && mobile.IsAlive)
				{
					if (source.InRange(mobile) && source.InLOS(mobile) && source.CanSee(mobile))
						_orb.ThrowAt(source, mobile);
				}
			}
			
			protected override void OnPath(MobileEntity source, List<Direction> path)
			{
				_orb.ThrowAt(source, source.Location + path);
			}
		}
	}
}