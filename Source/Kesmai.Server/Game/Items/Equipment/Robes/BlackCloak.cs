using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BlackCloak : Robe, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000026;

		/// <inheritdoc />
		public override uint BasePrice => 2400;

		/// <inheritdoc />
		public override int Weight => 1000;

		/// <inheritdoc />
		/// <remarks>Robes have a default <see cref="Hindrance"/> value of 1.</remarks>
		public override int Hindrance => 0;

		/// <inheritdoc />
		public override int ManaRegeneration => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlackCloak"/> class.
		/// </summary>
		public BlackCloak() : base(253)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200189)); /* [You are looking at] [a long black cloak.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250100)); /* The robe is extremely light. */
		}
	}
}