using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class WeakDexterityRing : DexterityRing, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int BonusDexterity => 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="WeakDexterityRing"/> class.
		/// </summary>
		public WeakDexterityRing() : base(55)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200049)); /* [You are looking at] [a flexible ring of interwoven gold wire.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250040)); /* The ring increases dexterity slightly. */
		}
	}
}
