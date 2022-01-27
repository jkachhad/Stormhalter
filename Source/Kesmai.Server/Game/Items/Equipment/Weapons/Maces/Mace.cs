using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Mace : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000060;

		/// <inheritdoc />
		public override int Category => 2;
		
		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <summary>
		/// Initializes a new instance of the <see cref="Mace"/> class.
		/// </summary>
		protected Mace(int maceID) : base(maceID)
		{
		}
	}
}
