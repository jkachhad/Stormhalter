using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class GiltDagger : Dagger
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int BaseArmorBonus => 1;

		/// <inheritdoc />
		public override int BaseAttackBonus => 3;
		
		/// <inheritdoc />
		protected override int PoisonedItemId => 312;

		/// <summary>
		/// Initializes a new instance of the <see cref="GiltDagger"/> class.
		/// </summary>
		public GiltDagger() : base(171)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200155)); /* [You are looking at] [a gilt-handled dagger with a fine steel blade.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250007)); /* The combat adds for this weapon are +3. */
		}
	}
}