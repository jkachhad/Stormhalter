using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class OysterPearl : Gem
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000067;
		
		/// <inheritdoc />
		public override int Weight => 5;

		/// <summary>
		/// Initializes a new instance of the <see cref="OysterPearl"/> class.
		/// </summary>
		[WorldForge]
		public OysterPearl(uint basePrice) : base(143, basePrice)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200229)); /* [You are looking at] [an oyster with a shiny pearl in it.] */
		}
	}
}