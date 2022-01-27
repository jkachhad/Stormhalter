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
	public partial class SilverDaggerAmulet : Amulet, ITreasure, ICharged
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
		public override uint BasePrice => 1000;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 100;
		
		public SilverDaggerAmulet() : this(20)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SilverDaggerAmulet"/> class.
		/// </summary>
		public SilverDaggerAmulet(int charges = 20) : base(304)
		{
			_chargesCurrent = charges;
			_chargesMax = charges;
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200072)); /* [You are looking at] [a small silver dagger with an emerald blade hanging from a silver chain.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250051)); /* The amulet contains the spell of Protection from Poison. */
		}

		protected override bool OnEquip(MobileEntity entity)
		{
			if (!base.OnEquip(entity))
				return false;

			if (_chargesCurrent > 0)
			{

				if (!entity.GetStatus(typeof(PoisonProtectionStatus), out var status))
				{
					status = new PoisonProtectionStatus(entity)
					{
						Inscription = new SpellInscription() { SpellId = 84 }
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

			if (entity.GetStatus(typeof(PoisonProtectionStatus), out var status))
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