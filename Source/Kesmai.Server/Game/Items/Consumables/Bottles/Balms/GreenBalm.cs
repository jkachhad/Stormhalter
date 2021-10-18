using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class GreenBalm : Balm
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GreenBalm"/> class.
		/// </summary>
		public GreenBalm() : base(247, 94)
		{
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200104)); /* [You are looking at] [a green glass bottle.] */

			base.GetDescription(entries);
		}
	}
}
