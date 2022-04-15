using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WizardHat : Helmet, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000103;

		/// <inheritdoc />
		public override uint BasePrice => 3000;

		/// <inheritdoc />
		public override int Weight => 80;
		
		/// <inheritdoc />
		public override int ProtectionFromStun => 10;
		
		/// <inheritdoc />
		
	    public override int MagicProtection => 15;
		
		/// <inheritdoc />
		public override bool ProvidesNightVision => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="WizardHat"/> class.
		/// </summary>
		public WizardHat() : base(35)
		{
		}
		
		/// <inheritdoc />
		public override bool CanEquip(MobileEntity entity)
		{
			if (entity is PlayerEntity { Profession: Wizard })
				return true;

			return false;
		}
		

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200354)); /* [You are looking at] [a spectacular azure wizards hat with bright yellow stars.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250134)); /* The hat emenates power, and shimmers as you touch it. It contains the spell of Night Vision. */
		}
	}
}