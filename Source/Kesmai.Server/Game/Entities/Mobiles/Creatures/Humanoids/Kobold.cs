using System.IO;

using Kesmai.Server.Accounting;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public partial class Kobold : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Kobold"/> class.
		/// </summary>
		public Kobold()
		{
			Name = "kobold";
			Body = 20;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetDeathSound() => 106;
		public override int GetNearbySound() => 92;
		public override int GetAttackSound() => 99;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}
