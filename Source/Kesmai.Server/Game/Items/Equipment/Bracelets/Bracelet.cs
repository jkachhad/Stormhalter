using System.IO;

namespace Kesmai.Server.Items
{
	public abstract partial class Bracelet : Equipment
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000018;
		
		/// <inheritdoc />
		public override int Category => 7;

		/// <summary>
		/// Initializes a new instance of the <see cref="Bracelet"/> class.
		/// </summary>
		protected Bracelet(int braceletID) : base(braceletID)
		{
		}
	}
}
