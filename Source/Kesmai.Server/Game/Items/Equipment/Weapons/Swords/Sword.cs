using System.IO;

using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Sword : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000090;
		
		/// <inheritdoc />
		public override int Category => 2;
		
		/// <inheritdoc />
		public override Skill Skill => Skill.Sword;

		/// <summary>
		/// Initializes a new instance of the <see cref="Sword" /> class.
		/// </summary>
		protected Sword(int swordID) : base(swordID)
		{
		}
	}
}
