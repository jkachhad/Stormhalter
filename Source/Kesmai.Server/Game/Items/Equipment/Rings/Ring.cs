using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Ring : ItemEntity
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000073;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 4;

		/// <summary>
		/// Initializes a new instance of the <see cref="Ring"/> class.
		/// </summary>
		protected Ring(int ringID) : base(ringID)
		{
		}
	}
}
