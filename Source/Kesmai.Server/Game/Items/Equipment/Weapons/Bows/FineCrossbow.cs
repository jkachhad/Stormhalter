using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class FineCrossbow : Crossbow
	{
		/// <inheritdoc />
		public override uint BasePrice => 600;
		
		/// <inheritdoc />
		public override int BaseAttackBonus => 2;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FineCrossbow"/> class.
		/// </summary>
		public FineCrossbow() : base(230)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200282)); /* [You are looking at] [a fine crossbow.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250080)); /* The combat adds for this weapon are +2. */
		}
	}
}