using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SteelLongsword : Longsword
	{
		/// <inheritdoc />
		public override uint BasePrice => 1500;
		
		/// <inheritdoc />
		public override int BaseAttackBonus => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="SteelLongsword"/> class.
		/// </summary>
		public SteelLongsword() : base(170)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200154)); /* [You are looking at] [a shiny steel longsword.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250003)); /* The combat adds for this weapon are +4. */
		}
	}
}