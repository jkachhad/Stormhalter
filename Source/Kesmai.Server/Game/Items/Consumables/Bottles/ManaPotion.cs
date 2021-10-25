using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class ManaPotion : Bottle, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 500;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="ManaPotion"/> class.
		/// </summary>
		public ManaPotion() : this(6, 6)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="ManaPotion"/> class.
		/// </summary>
		public ManaPotion(int closedId, int openId) : base(closedId)
		{
		}
		
		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = ConsumableRestoreMana.Full;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200085)); /* [You are looking at] [a white porcelain bottle adorned with lotus blossoms.] */

			base.GetDescription(entries);
		}
	}
}