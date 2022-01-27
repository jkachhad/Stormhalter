using System.Collections.Generic;
using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Network;

namespace Kesmai.Server.Items
{
	public partial class DexterityRing : Ring, ITreasure
	{
		/// <inheritdoc />
		public override uint BasePrice => 1500;

		/// <inheritdoc />
		public override int Weight => 20;

		/// <summary>
		/// The dexterity bonus provided by this ring.
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int BonusDexterity => 2;

		/// <summary>
		/// Initializes a new instance of the <see cref="DexterityRing"/> class.
		/// </summary>
		public DexterityRing() : this(19)
		{
		}
		
		/// <summary>
        /// Initializes a new instance of the <see cref="DexterityRing"/> class.
        /// </summary>
        public DexterityRing(int ringId) : base(ringId)
        {
        }

		/// <inheritdoc />
		public override void GetDescription(List<LocalizationEntry> entries)
		{
			entries.Add(new LocalizationEntry(6200000, 6200056)); /* [You are looking at] [a wide platinum band engraved with many tiny runes.] */

			if (Identified)
				entries.Add(new LocalizationEntry(6250044)); /* The ring greatly increases dexterity. */
		}
	}
}
