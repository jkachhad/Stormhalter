using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class MisericordeDagger : Dagger, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int BaseAttackBonus => 3;

		/// <inheritdoc />
		public override WeaponFlags Flags => base.Flags | WeaponFlags.BlueGlowing | WeaponFlags.Neutral;

        /// <inheritdoc />
		public override bool CanBind => true;

        /// <inheritdoc />
		protected override int PoisonedItemId => 310;

		/// <summary>
		/// Initializes a new instance of the <see cref="MisericordeDagger"/> class.
		/// </summary>
		public MisericordeDagger() : base(186)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200159)); /* [You are looking at] [an exquisite lemurian misericorde dagger whose honed finish reveals the wood grained texture of the metal. The dagger is emitting a faint blue glow. The weapon is neutral.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250007)); /* The combat adds for this weapon are +3. */
		}
	}
}