using System.IO;

namespace Kesmai.Server.Game
{
	public partial class AirElemental : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AirElemental"/> class.
		/// </summary>
		public AirElemental()
		{
			Name = "elemental";
			Body = 26;

			Alignment = Alignment.Chaotic;
			
			CanFly = true;
			CanLoot = false;
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();

			if (_brain != null)
				return;

			_brain = new CombatAI(this);
		}

		public override int GetNearbySound() => 115;
		public override int GetAttackSound() => 72;
		public override int GetDeathSound() => 72;
	}
}