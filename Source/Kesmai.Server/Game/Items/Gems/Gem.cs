using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Gem : ItemEntity, ITreasure
	{
		protected uint _basePrice;
		
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000042;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 3;

		public override uint BasePrice => _basePrice;

		/// <summary>
		/// Initializes a new instance of the <see cref="Gem"/> class.
		/// </summary>
		[WorldForge]
		protected Gem(int gemID, uint basePrice) : base(gemID)
		{
			_basePrice = basePrice;
		}
	}
}
