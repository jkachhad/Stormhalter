using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class LeatherArmor : Armor
	{
		/// <inheritdoc />
		public override uint BasePrice => 25;

		/// <inheritdoc />
		public override int Weight => 1500;

		/// <inheritdoc />
		public override int Hindrance => 1;

		/// <inheritdoc />
		public override int SlashingProtection => 1;

		/// <inheritdoc />
		public override int BashingProtection => 1;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="LeatherArmor"/> class.
		/// </summary>
		public LeatherArmor() : base(242)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200002)); /* [You are looking at] [a suit of leather armor.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250001)); /* The armor seems quite ordinary. */
		}
	}
}
