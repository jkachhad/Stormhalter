using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game
{
	public partial class WaterPotion : Bottle
	{
		private static ConsumableWater content = new ConsumableWater();
		
		/// <inheritdoc />
		public override uint BasePrice => 5;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaterPotion"/> class.
		/// </summary>
		public WaterPotion() : base(210)
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
			entries.Add(new LocalizationEntry(6200000, 6200087)); /* [You are looking at] [a clear glass bottle.  Inside you see a clear liquid.] */

			base.GetDescription(entries);
		}
	}
}