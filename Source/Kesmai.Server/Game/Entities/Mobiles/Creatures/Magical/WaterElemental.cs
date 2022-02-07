using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class WaterElemental : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WaterElemental"/> class.
		/// </summary>
		public WaterElemental()
		{
			Name = "elemental";
			Body = 85;

			Alignment = Alignment.Chaotic;
			CanSwim = true;
			AddStatus(new BreatheWaterStatus(this));
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();
			
			if (_brain != null)
				return;
			
			_brain = new CombatAI(this);
		}
		
		public override int GetNearbySound() => 133;
		public override int GetAttackSound() => 152;
		public override int GetDeathSound() => 171;
	}
}