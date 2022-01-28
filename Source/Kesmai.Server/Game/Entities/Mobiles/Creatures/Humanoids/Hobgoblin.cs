using System.IO;
using System.Linq;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Hobgoblin : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Hobgoblin"/> class.
		/// </summary>
		public Hobgoblin()
		{
			Name = "hobgoblin";
			Body = 78;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
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
		public override int GetDeathSound() => 108;
		public override int GetNearbySound() => 94;
		public override int GetAttackSound() => 101;
		
		public override ItemEntity OnCorpseTanned()
		{
			return new LeatherArmor();
		}
	}
}