using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Currency : ItemEntity, ITreasure
	{
		/// <summary>
		/// Gets or sets a value indicating whether this instance is stackable.
		/// </summary>
		public override bool Stackable => true;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 13;

		/// <summary>
		/// Initializes a new instance of the <see cref="Currency"/> class.
		/// </summary>
		protected Currency(int currencyID) : base(currencyID)
		{
		}
	}
}
