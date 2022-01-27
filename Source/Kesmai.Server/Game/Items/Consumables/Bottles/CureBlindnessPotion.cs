using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class CureBlindnessPotion : Bottle
	{
		private static ConsumableBlindnessAntidote content = new ConsumableBlindnessAntidote();
		
		/// <inheritdoc />
		public override uint BasePrice => 25;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="CureBlindnessPotion"/> class.
		/// </summary>
		public CureBlindnessPotion() : base(212)
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
			entries.Add(new LocalizationEntry(6200000, 6200091)); /* [You are looking at] [a clear bottle with a brown label.] */

			base.GetDescription(entries);
		}
	}
}