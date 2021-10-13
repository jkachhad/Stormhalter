using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class ClearBalm : Balm
	{
		/// <inheritdoc />
		public override uint BasePrice => 16;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="ClearBalm"/> class.
		/// </summary>
		public ClearBalm() : base(210, 93)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200005)); /* [You are looking at] [a clear glass bottle.  Inside you see a cloudy white liquid.] */

			base.GetDescription(entries);
		}
	}
}
