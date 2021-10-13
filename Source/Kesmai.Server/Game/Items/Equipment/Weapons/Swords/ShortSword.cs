using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class ShortSword : Sword
	{
		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 1200;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Medium;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 6;
		
		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Piercing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ShortSword"/> class.
		/// </summary>
		public ShortSword() : this(88)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ShortSword"/> class.
		/// </summary>
		public ShortSword(int swordId) : base(swordId)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200024)); /* [You are looking at] [an iron shortsword.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250020)); /* The shortsword appears quite ordinary. */
		}
	}
}