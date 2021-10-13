using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlackSteelRapier : Rapier, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;
		
		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing;

		/// <inheritdoc />
		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlackSteelRapier"/> class.
		/// </summary>
		public BlackSteelRapier() : base(309)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200280)); /* [You are looking at] [an impossibly thin black blade mounted on a silver hilt.  The rapier is emitting a faint blue glow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}
	}
}