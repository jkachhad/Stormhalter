using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class LocateAmulet : Amulet, ITreasure, ICharges
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LocateAmulet"/> class.
		/// </summary>
		protected LocateAmulet(int amuletId, int charges = 3) : base(amuletId)
		{
			_charges = charges;
		}
		
		/// <inheritdoc />
		public override ActionType GetAction()
		{
			var container = Container;
			
			if (container is Hands || container is Paperdoll) 
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
			
			if (_charges > 0)
			{
				var spell = new InstantLocateSpell()
				{
					Item = this,
					Cost = 0,
				};

				spell.Warm(entity);
				spell.Cast();
			}

			return true;
		}
	}
}