using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game
{
	public partial class StaminaPotion : Bottle
	{
		/// <inheritdoc />
		public override uint BasePrice => 40;

		/// <inheritdoc />
		public override int Weight => 240;

		/// <summary>
		/// Initializes a new instance of the <see cref="StaminaPotion"/> class.
		/// </summary>
		public StaminaPotion() : base(214)
		{
		}
		
		/// <inheritdoc />
		protected override void OnCreate()
		{
			base.OnCreate();

			if (_content is null)
				_content = ConsumableRestoreStamina.Full;
		}
		
		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200093)); /* [You are looking at] [a pale-blue bottle streaked with white lines.] */

			base.GetDescription(entries);
		}
	}
}