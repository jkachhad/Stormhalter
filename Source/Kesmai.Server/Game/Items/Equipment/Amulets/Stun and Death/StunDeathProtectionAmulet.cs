using System;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public abstract partial class StunDeathProtectionAmulet : Amulet, ITreasure, ICharged
	{
		private int _chargesCurrent;
		private int _chargesMax;

		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesCurrent
		{
			get => _chargesCurrent;
			set => _chargesCurrent = value.Clamp(0, _chargesMax);
		}
		
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public int ChargesMax
		{
			get => _chargesMax;
			set => _chargesMax = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StunDeathProtectionAmulet"/> class.
		/// </summary>
		protected StunDeathProtectionAmulet(int amuletId, int charges = 3) : base(amuletId)
		{
			_chargesCurrent = charges;
			_chargesMax = charges;
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (_chargesCurrent > 0)
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
				if (_chargesCurrent > 0)
					_chargesCurrent--;
			}
		}
	}
}