using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class OysterShell : Gem
	{
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000081;

		/// <summary>
		/// Initializes a new instance of the <see cref="OysterShell"/> class.
		/// </summary>
		[WorldForge]
		public OysterShell(uint basePrice) : base(52, basePrice)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200287)); /* [You are looking at] [an oyster shell.] */
		}
	}
}