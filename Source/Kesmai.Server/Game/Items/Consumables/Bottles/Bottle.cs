using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public abstract partial class Bottle : Consumable
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000013;

		/// <inheritdoc />
		public override int Category => 12;

		/// <summary>
		/// Initializes a new instance of the <see cref="Bottle"/> class.
		/// </summary>
		public Bottle(int closedId) : base(closedId)
		{
		}

		public override void GetDescription(List<LocalizationEntry> entries)
		{
			base.GetDescription(entries);

			if (IsOpen)
				entries.Add(new LocalizationEntry(6300383)); /* The container is open. */
		}
	}
}
