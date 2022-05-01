using System;
using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class PowerBracelet : Bracelet, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1;

		/// <inheritdoc />
		public override int Weight => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="PowerBracelet"/> class.
		/// </summary>
		public PowerBracelet() : base(135)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200355)); /* [You are looking at] [a golden bracelet embued with sapphires.] */
		}

        /// <summary>
		/// Initializes a new instance of the <see cref="PowerBracelet"/> class.
		/// </summary>
		public PowerBracelet(Serial serial) : base(serial)
		{
		}
	}
}
