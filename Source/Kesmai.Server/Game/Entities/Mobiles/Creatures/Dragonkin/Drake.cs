using System;
using System.IO;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Drake : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Drake"/> class.
		/// </summary>
		public Drake()
		{
			Name = "drake";
			Body = 93;

			Alignment = Alignment.Evil;
			CombatantChangeInterval = TimeSpan.FromSeconds(33.0);
			
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

		/// <summary>
		/// Gets the death sound.
		/// </summary>
		public override int GetDeathSound() => 43;

		public override int GetNearbySound() => 19;
		public override int GetAttackSound() => 31;

		public override Corpse GetCorpse()
		{
			var corpse = base.GetCorpse();

			if (corpse != null)
				corpse.CanBurn = false;

			return corpse;
		}

		public override ItemEntity OnCorpseTanned()
		{
			return new DrakeScaleArmor();
		}
	}
}