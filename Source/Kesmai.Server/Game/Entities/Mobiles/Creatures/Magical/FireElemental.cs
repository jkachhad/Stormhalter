using System.IO;

namespace Kesmai.Server.Game
{
	public partial class FireElemental : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FireElemental"/> class.
		/// </summary>
		public FireElemental()
		{
			Name = "elemental";
			Body = 223;

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

		//TODO: check if there's a better sound for this guy: Using Water Elemental stats for now.
		public override int GetNearbySound() => 69;
		public override int GetAttackSound() => 69;
		public override int GetDeathSound() => 224;
	}
}