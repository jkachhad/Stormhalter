using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Statue : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Statue"/> class.
		/// </summary>
		public Statue()
		{
			Name = "statue";
			Body = 99;

			Alignment = Alignment.Chaotic;
			
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
		
		public override int GetAttackSound() => 0;
		public override int GetNearbySound() => 0;
		public override int GetDeathSound() => 172;
		
		public override Corpse GetCorpse()
		{
			var corpse = base.GetCorpse();
			
			if (corpse != null)
				corpse.CanBurn = false;

			return corpse;
		}
	}
}