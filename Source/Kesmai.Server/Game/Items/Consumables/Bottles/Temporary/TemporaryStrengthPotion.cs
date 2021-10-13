using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class TemporaryStrengthPotion : Bottle, ITreasure
	{
		private static ConsumableStrengthSpell content = new ConsumableStrengthSpell();
		
		/// <inheritdoc />
		public override uint BasePrice => 200;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="TemporaryStrengthPotion"/> class.
		/// </summary>
		public TemporaryStrengthPotion() : this(211, 41)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="TemporaryStrengthPotion"/> class.
		/// </summary>
		public TemporaryStrengthPotion(int closedId, int openId) : base(closedId)
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
			entries.Add(new LocalizationEntry(6200000, 6200090)); /* [You are looking at] [a clear bottle made of yellowish glass.  Inside you see a red liquid.] */

			base.GetDescription(entries);
		}
	}
}