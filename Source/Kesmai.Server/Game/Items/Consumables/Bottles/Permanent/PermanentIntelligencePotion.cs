using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class PermanentIntelligencePotion : Bottle, ITreasure
	{
		private static ConsumableIntelligenceStat content = new ConsumableIntelligenceStat();
		
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 240;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentIntelligencePotion"/> class.
		/// </summary>
		public PermanentIntelligencePotion() : base(216)
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
			entries.Add(new LocalizationEntry(6200000, 6200095)); /* [You are looking at] [a porcelain bottle stamped with a golden sun.] */

			base.GetDescription(entries);
		}
	}
}