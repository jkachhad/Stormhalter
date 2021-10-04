using System.IO;

namespace Kesmai.Server.Items
{
	public partial class Boots : Equipment
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000012;
		
		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 11;

		/// <summary>
		/// Initializes a new instance of the <see cref="Boots"/> class.
		/// </summary>
		public Boots(int bootsID) : base(bootsID)
		{
		}
	}
}
