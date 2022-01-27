using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class Yttril : ItemEntity, ITreasure
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000075;

		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 1200;

		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 2000;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 3;
		/// <summary>
		/// Initializes a new instance of the <see cref="Yttril"/> class.
		/// </summary>
		public Yttril() : base(110)
		{
		}
		
		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200221)); /* [You are looking at] [a blue_gray stone flecked with bits of glowing metal.  The stone is known as yttril.] */
		}
	}
}