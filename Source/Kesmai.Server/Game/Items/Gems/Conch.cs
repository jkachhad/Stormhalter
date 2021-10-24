using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Conch : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000029;

		/// <summary>
		/// Initializes a new instance of the <see cref="Conch"/> class.
		/// </summary>
		[WorldForge]
		public Conch(uint basePrice) : base(156, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200288)); /* [You are looking at] [a conch.] */
		}
	}
}