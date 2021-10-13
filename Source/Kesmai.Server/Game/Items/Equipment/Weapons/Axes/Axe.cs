using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Axe : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000005;
		
		/// <inheritdoc />
		public override int Category => 2;

		/// <inheritdoc />
		public override Skill Skill => Skill.Mace;

		/// <summary>
		/// Initializes a new instance of the <see cref="Axe"/> class.
		/// </summary>
		protected Axe(int axeID) : base(axeID)
		{
		}
	}
}
