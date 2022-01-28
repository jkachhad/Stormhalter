using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Crocodile : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Crocodile"/> class.
		/// </summary>
		public Crocodile()
		{
			Name = "crocodile";
			Body = 18;

			Alignment = Alignment.Chaotic;

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
		public override int GetNearbySound() => 8;
		public override int GetAttackSound() => 20;
		public override int GetDeathSound() => 32;
		
		public override ItemEntity OnCorpseTanned()
        {
        	return new CrocodileBoots();
        }
	}
}