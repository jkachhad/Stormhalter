using System.IO;

namespace Kesmai.Server.Items
{
	public abstract partial class Amulet : Equipment
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000002;
		
		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 6;

		/// <summary>
		/// Initializes a new instance of the <see cref="Amulet"/> class.
		/// </summary>
		protected Amulet(int amuletId) : base(amuletId)
		{
		}
	}
}
