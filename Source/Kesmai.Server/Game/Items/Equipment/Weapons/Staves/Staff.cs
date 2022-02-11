using System.IO;
using Kesmai.Server.Game;

namespace Kesmai.Server.Items
{
	public abstract partial class Staff : MeleeWeapon
	{
		/// <inheritdoc />
		public override int LabelNumber => 6000088;

		/// <inheritdoc />
		public override int Category => 1;
		
		/// <inheritdoc />
		public override Skill Skill => Skill.Staff;

		/// <inheritdoc />
		public override ShieldPenetration Penetration => ShieldPenetration.Light;

		/// <summary>
		/// Initializes a new instance of the <see cref="Staff"/> class.
		/// </summary>
		protected Staff(int staffID) : base(staffID)
		{
		}
	}
}
