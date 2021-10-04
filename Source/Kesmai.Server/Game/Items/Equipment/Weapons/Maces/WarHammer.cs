using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WarHammer : Mace
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000047;

		/// <inheritdoc />
		public override uint BasePrice => 20;

		/// <inheritdoc />
		public override int Weight => 2000;

		/// <inheritdoc />
		public override int MinimumDamage => 2;

		/// <inheritdoc />
		public override int MaximumDamage => 8;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <inheritdoc />
		public override WeaponFlags Flags => WeaponFlags.Throwable | WeaponFlags.Bashing;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="WarHammer"/> class.
		/// </summary>
		public WarHammer() : base(112)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200142)); /* [You are looking at] [a steel war hammer.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250079)); /* The hammer appears quite ordinary. */
		}
	}
}