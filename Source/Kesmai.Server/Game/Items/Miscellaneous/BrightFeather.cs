using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class BrightFeather : ItemEntity, ITreasure
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000101;

		/// <inheritdoc />
		public override uint BasePrice => 300;

		/// <inheritdoc />
		public override bool CanBind => true;

		public BrightFeather() : base(397)
		{
		}

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200285)); /* [You are looking at] [a bright feather.] */
		}
	}
}