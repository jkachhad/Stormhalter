using System;
using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class PermanentConstitutionPotion : Bottle, ITreasure
	{
		private static ConsumableConstitutionStat content = new ConsumableConstitutionStat();
		
		/// <inheritdoc />
		public override uint BasePrice => 1000;

		/// <inheritdoc />
		public override int Weight => 240;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PermanentConstitutionPotion"/> class.
		/// </summary>
		public PermanentConstitutionPotion() : base(303)
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
			entries.Add(new LocalizationEntry(6200000, 6200113)); /* [You are looking at] [a small porcelain vial.] */

			base.GetDescription(entries);
		}
	}
}
