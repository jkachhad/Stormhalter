using System.IO;
using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;

namespace Kesmai.Server.Items
{
	public abstract partial class Robe : Equipment
	{
		/// <summary>
		/// Gets the label number.
		/// </summary>
		public override int LabelNumber => 6000074;

		/// <summary>
		/// Gets the hindrance penalty for this <see cref="Armor"/>.
		/// </summary>
		/// <remarks>
		/// All robes have some hindrance with the exception of <see cref="Kimono">Kimonos</see>.
		/// </remarks>
		public override int Hindrance => 1;

		/// <summary>
		/// Gets the item category.
		/// </summary>
		public override int Category => 10;
		
		/// <summary>
		/// Gets the mana regeneration provided by this <see cref="Robe"/>
		/// </summary>
		[WorldForge]
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ManaRegeneration => 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="Robe"/> class.
		/// </summary>
		protected Robe(int robeID) : base(robeID)
		{
		}
	}
}
