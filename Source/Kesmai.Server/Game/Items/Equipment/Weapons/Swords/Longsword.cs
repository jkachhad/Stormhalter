using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Longsword : Sword
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000059;

		/// <inheritdoc />
		public override uint BasePrice => 30;

		/// <inheritdoc />
		public override int Weight => 1450;

		/// <inheritdoc />
		public override int MinimumDamage => 1;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Sword;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Slashing;

		/// <summary>
		/// Initializes a new instance of the <see cref="Longsword"/> class.
		/// </summary>
		public Longsword() : this(85)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Longsword"/> class.
		/// </summary>
		public Longsword(int itemId) : base(itemId)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200014)); /* [You are looking at] [a fine longsword.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250013)); /* The longsword appears quite ordinary. */
		}
	}
}