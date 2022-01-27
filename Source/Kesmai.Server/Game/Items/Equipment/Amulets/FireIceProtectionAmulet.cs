using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class FireIceProtectionAmulet : Amulet, ITreasure, ICharged
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
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 800;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="FireIceProtectionAmulet"/> class.
		/// </summary>
		public FireIceProtectionAmulet() : this(3)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FireIceProtectionAmulet"/> class.
		/// </summary>
		public FireIceProtectionAmulet(int charges) : base(5)
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
				if (!entity.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
				{
					fireStatus = new FireProtectionStatus(entity)
					{
						Inscription = new SpellInscription() { SpellId = 43 }
					};
					fireStatus.AddSource(new ItemSource(this));

					entity.AddStatus(fireStatus);
				}
				else
				{
					fireStatus.AddSource(new ItemSource(this));
				}

				if (!entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
				{
					iceStatus = new IceProtectionStatus(entity)
					{
						Inscription = new SpellInscription() { SpellId = 42 }
					};
					iceStatus.AddSource(new ItemSource(this));

					entity.AddStatus(iceStatus);
				}
				else
				{
					iceStatus.AddSource(new ItemSource(this));
				}
			}

			return true;
		}

		protected override bool OnUnequip(MobileEntity entity)
		{
			if (!base.OnUnequip(entity))
				return false;

			if (entity.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
				fireStatus.RemoveSourceFor(this);
			
			if (entity.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
				iceStatus.RemoveSourceFor(this);

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
