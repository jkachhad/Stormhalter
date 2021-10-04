using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class YellowBalm : Balm
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="YellowBalm"/> class.
		/// </summary>
		public YellowBalm() : base(211, 41)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200088)); /* [You are looking at] [a clear bottle made of yellowish glass.  Inside you see a cloudy white liquid.] */

			base.GetDescription(entries);
		}
	}
}
