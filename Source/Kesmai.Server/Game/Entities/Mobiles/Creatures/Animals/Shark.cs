using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class Shark : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Shark"/> class.
		/// </summary>
		public Shark()
		{
			Name = "shark";
			Body = 54;

			Alignment = Alignment.Chaotic;

			CanSwim = true;
			CanWalk = false;
			
			AddStatus(new BreatheWaterStatus(this));
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			_brain = new CombatAI(this);

			base.OnLoad();
		}

		public override int GetNearbySound() => 2001;
		public override int GetDeathSound() => 171;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new SharkJacket();
		}
	}
}