using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class SteelDagger : Dagger
	{
		/// <inheritdoc />
		public override uint BasePrice => 10;
		
		/// <inheritdoc />
		protected override int PoisonedItemId => 313;

		/// <summary>
		/// Initializes a new instance of the <see cref="SteelDagger"/> class.
		/// </summary>
		public SteelDagger() : base(101)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200015)); /* [You are looking at] [a steel dagger.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250014)); /* The dagger appears quite ordinary. */
		}
	}
}