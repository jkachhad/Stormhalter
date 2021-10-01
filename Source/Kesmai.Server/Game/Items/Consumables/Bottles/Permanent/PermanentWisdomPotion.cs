using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class PermanentWisdomPotion : Bottle, ITreasure
	{
		private static ConsumableWisdomStat content = new ConsumableWisdomStat();
		
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 240;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentWisdomPotion"/> class.
		/// </summary>
		public PermanentWisdomPotion() : base(274)
		{
		}
		
		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = content;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200107)); /* [You are looking at] [a heavy glass bottle mounted on a gold base and set with emeralds.] */

			base.GetDescription(entries);
		}
	}
}
