using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Presence : CreatureEntity, IUndead
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Presence"/> class.
		/// </summary>
		public Presence()
		{
			Name = "presence";
			Body = 41;

			Alignment = Alignment.Chaotic;
		}

		public override void OnSpawn()
		{
			base.OnSpawn();
			
			if (_brain != null)
				return;
			
			if (RightHand is ProjectileWeapon)
				_brain = new RangedAI(this);
			else
				_brain = new CombatAI(this);
		}

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetNearbySound() => 115;
		public override int GetAttackSound() => 122;
		public override int GetDeathSound() => 5;

		public override Corpse GetCorpse() => default(Corpse);
	}
}