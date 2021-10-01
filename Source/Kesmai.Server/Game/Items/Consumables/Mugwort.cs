using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Items
{
	public partial class Mugwort : Consumable
	{
		/* Mugwort cures to a absolute potency of 5. */
		private static ConsumablePoisonAntidote content = new ConsumablePoisonAntidote(5, false);
		
		/// <inheritdoc />
		public override int LabelNumber => 6000087;

		/// <inheritdoc />
		public override uint BasePrice => 25;

		/// <inheritdoc />
		public override int Weight => 100;

		/// <summary>
		/// Initializes a new instance of the <see cref="Mugwort"/> class.
		/// </summary>
		public Mugwort() : base(138)
		{
		}

		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = content;
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200231)); /* [You are looking at] [a dark-green sprig of mugwort.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250119)); /* The sprig contains the spell of Neutralize. */
		}
	}
}
