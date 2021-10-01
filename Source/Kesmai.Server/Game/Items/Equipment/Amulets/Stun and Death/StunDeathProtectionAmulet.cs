using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class StunDeathProtectionAmulet : Amulet, ITreasure, ICharges
	{
		private int _charges;

		public int Charges
		{
			get => _charges;
			set => _charges = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StunDeathProtectionAmulet"/> class.
		/// </summary>
		protected StunDeathProtectionAmulet(int amuletId, int charges = 3) : base(amuletId)
		{
			_charges = charges;
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (_charges > 0)
			{
				if (!entity.GetStatus(typeof(StunDeathProtectionStatus), out var status))
				{
					status = new StunDeathProtectionStatus(entity)
					{
						Inscription = new SpellInscription() { SpellId = 45 }
					};
					status.AddSource(new ItemSource(this));

					entity.AddStatus(status);
				}
				else
				{
					status.AddSource(new ItemSource(this));
				}
			}

			return true;
		}
		
		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(StunDeathProtectionStatus), out var status))
				status.RemoveSourceFor(this);

			return true;
		}
		
		public override void OnStrip(Corpse corpse)
		{
			/* Only reduce charges if the item was stripped when on paperdoll or rings. */
			if (Container is EquipmentContainer)
			{
				if (_charges > 0)
					_charges--;
			}
		}
	}
}