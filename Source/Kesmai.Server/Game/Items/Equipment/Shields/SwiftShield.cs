using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class SwiftShield : Shield, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 3000;

		/// <inheritdoc />
		public override int Category => 1;

        /// <summary>
		/// Gets or sets the shield-value provided by this ring.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Shield => 3;
		
		/// <inheritdoc />
		public override int BaseArmorBonus => 2;

		/// <inheritdoc />
		public override int ProjectileProtection => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="SwiftShield"/> class.
		/// </summary>
		public SwiftShield() : base(76)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200215)); /* [You are looking at] [a steel shield adorned with a black lightning bolt.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250109)); /* The shield contains the spell of Lightning Resist. */
		}


		public override void OnWield(MobileEntity entity)
		{
			base.OnWield(entity);

			if (!entity.GetStatus(typeof(ShieldStatus), out var status))
			{
				status = new ShieldStatus(entity)
				{
					Inscription = new SpellInscription() { SpellId = 52 }
				};
				status.AddSource(new ItemSource(this));
				
				entity.AddStatus(status);
			}
			else
			{
				status.AddSource(new ItemSource(this));
			}

			return true;
		}

		public override void OnUnwield(MobileEntity entity)
		{
			base.OnUnequip(entity);

			if (entity.GetStatus(typeof(ShieldStatus), out var status))
				status.RemoveSourceFor(this);
		}
	}
}
