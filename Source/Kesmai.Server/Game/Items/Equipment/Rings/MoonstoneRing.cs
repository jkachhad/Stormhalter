using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class MoonstoneRing : Ring
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 100;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MoonstoneRing"/> class.
		/// </summary>
		public MoonstoneRing() : base(57)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200047)); /* [You are looking at] [a silver ring set with a pale, milky moonstone.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250039)); /* The ring appears to be nothing special. */
		}
	}
}
