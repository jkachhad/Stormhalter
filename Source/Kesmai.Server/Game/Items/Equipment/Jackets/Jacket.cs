using System.IO;

namespace Kesmai.Server.Items
{
	public abstract partial class Jacket : Robe
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000052; /* jacket */
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Jacket"/> class.
		/// </summary>
		protected Jacket(int jacketId) : base(jacketId)
		{
		}
	}
}