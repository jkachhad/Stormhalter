using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class PermanentStrengthPotion : Bottle, ITreasure
	{
		private static ConsumableStrengthStat content = new ConsumableStrengthStat();
		
		/// <inheritdoc />
		public override uint BasePrice => 1000;
		
		/// <inheritdoc />
		public override int Weight => 240;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentStrengthPotion"/> class.
		/// </summary>
		public PermanentStrengthPotion() : base(225)
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
			entries.Add(new LocalizationEntry(6200000, 6200099)); /* [You are looking at] [a reddish clay bottle with three black concentric circles painted on one side.] */

			base.GetDescription(entries);
		}
	}
}
