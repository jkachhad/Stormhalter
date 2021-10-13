using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class HummingbirdSword : ShortSword, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;
		
		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Silver | WeaponFlags.Lawful;
		
		/// <inheritdoc />
		public override bool CanBind => true;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="HummingbirdSword"/> class.
		/// </summary>
		public HummingbirdSword() : base(306)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200281)); /* [You are looking at] [a fine sword made of an otherworldly metal. Looking at the blade makes you dizzy, as if you were looking at the wings of a hummingbird. The longsword is emitting a faint blue glow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}

#if (Alpha)
		protected override bool OnDroppedInto(MobileEntity entity, Container container, int slot)
		{
			if (entity != null)
				entity.Delta(MobileDelta.Body);
			
			return base.OnDroppedInto(entity, container, slot);
		}
#endif

		public override TimeSpan GetSwingDelay(MobileEntity entity)
		{
			return entity.GetRoundDelay(0.5);
		}
	}
}