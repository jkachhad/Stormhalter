using System.Collections.Generic;
using System.IO;

using Kesmai.Server.Game;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class KnightRing : Ring, ITreasure
	{
		/// <summary>
		/// Gets the price.
		/// </summary>
		public override uint BasePrice => 150;
		
		/// <summary>
		/// Gets the weight.
		/// </summary>
		public override int Weight => 20;

		public override bool CanBind => true;

		/// <summary>
		/// Initializes a new instance of the <see cref="KnightRing"/> class.
		/// </summary>
		public KnightRing() : base(107)
		{
		}

		/// <summary>
		/// Gets the description for this instance.
		/// </summary>
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200054)); /* [You are looking at] [a large silver ring, with a triangular piece of jade mounted among three small blue diamonds.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250043)); /* The ring contains Knight's spells. */
		}

		protected override bool OnDroppedInto(MobileEntity entity, Container container, int slot)
		{
			if (!base.OnDroppedInto(entity, container, slot))
				return false;

			return true;
		}
	}
}
