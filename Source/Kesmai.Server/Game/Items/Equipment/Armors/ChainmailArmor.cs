using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class ChainmailArmor : Armor
	{
		/// <inheritdoc />
		public override uint BasePrice => 50;

		/// <inheritdoc />
		public override int Weight => 2800;

		/// <inheritdoc />
		public override int Hindrance => 1;
		
		/// <inheritdoc />
		public override int SlashingProtection => 2;

		/// <inheritdoc />
		public override int PiercingProtection => 1;

		/// <inheritdoc />
		public override int BashingProtection => 1;
		
		/// <inheritdoc />
		public override bool BlockSpellcast => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="ChainmailArmor"/> class.
		/// </summary>
		public ChainmailArmor() : base(239)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200027)); /* [You are looking at] [a suit of chainmail.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250022)); /* The armor appears quite ordinary. */
		}
	}
}
