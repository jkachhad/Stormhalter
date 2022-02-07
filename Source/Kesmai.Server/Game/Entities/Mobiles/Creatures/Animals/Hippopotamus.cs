using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Hippopotamus : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Hippopotamus"/> class.
		/// </summary>
		public Hippopotamus()
		{
			Name = "hippopotamus";
			Body = 44;

			Alignment = Alignment.Lawful;

			CanSwim = true;
			AddStatus(new BreatheWaterStatus(this));
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
		public override int GetNearbySound() => 0;
		public override int GetAttackSound() => 32;
		public override int GetDeathSound() => 0;

		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}