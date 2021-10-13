using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class PermanentManaPotion : Bottle, ITreasure
	{
		private static ConsumableIncreaseMana content = new ConsumableIncreaseMana(6);
		
		/// <inheritdoc />
		public override uint BasePrice => 2000;

		/// <inheritdoc />
		public override int Weight => 240;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentManaPotion"/> class.
		/// </summary>
		public PermanentManaPotion() : base(316)
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
			entries.Add(new LocalizationEntry(6200000, 6200114)); /* [You are looking at] [a sky-blue porcelain bottle.] */

			base.GetDescription(entries);
		}
	}
}
