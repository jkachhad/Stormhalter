using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game
{
	public partial class NitroPotion : Bottle
	{
		private static ConsumableNitro content = new ConsumableNitro();
		
		/// <inheritdoc />
		public override uint BasePrice => 100;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="NitroPotion"/> class.
		/// </summary>
		public NitroPotion() : base(209)
		{
		}

		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = content;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200087)); /* [You are looking at] [a clear glass bottle.] */

			base.GetDescription(entries);
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

		private class InternalTarget : Target
		{
			private NitroPotion _potion;
			
			public InternalTarget(NitroPotion potion) : base(4, TargetFlags.Path | TargetFlags.Mobiles | TargetFlags.Harmful)
			{
				_potion = potion;
			}
			
			protected override void OnTarget(MobileEntity source, object target)
			{
				if (target is MobileEntity mobile && mobile.IsAlive)
				{
					if (source.InRange(mobile) && source.InLOS(mobile) && source.CanSee(mobile))
						_potion.ThrowAt(source, mobile);
				}
			}
			
			protected override void OnPath(MobileEntity source, List<Direction> path)
			{
				_potion.ThrowAt(source, source.Location + path);
			}
		}
	}
}