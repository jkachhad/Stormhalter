using System;
using System.IO;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game
{
	public partial class Dragon : CreatureEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Dragon"/> class.
		/// </summary>
		public Dragon()
		{
			Name = "dragon";
			Body = 92;

			Alignment = Alignment.Evil;
			CombatantChangeInterval = TimeSpan.FromSeconds(33.0); // TODO: Scale to facet time?
			
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
		
		public override int GetNearbySound() => 19;
		public override int GetAttackSound() => 31;
		public override int GetDeathSound() => 43;

		public override Corpse GetCorpse()
		{
			var corpse = base.GetCorpse();
			
			if (corpse != null)
				corpse.CanBurn = false;

			return corpse;
		}

		public override ItemEntity OnCorpseTanned()
		{
			return new DragonScaleArmor();
		}

		public override void OnSpellTarget(Target target, MobileEntity combatant)
		{
			if (Spell is DragonBreathSpell dragonBreath)
			{
				var direction = GetDirectionTo(combatant.Location);
				var distance = GetDistanceToMax(combatant.Location);
				
				if (direction == Direction.None)
					direction = Direction.Cardinal.Random();

				if (distance >= 3)
					dragonBreath.CastAt(direction);
				else
					dragonBreath.CastAt(direction, direction.Opposite);

				if (target != null)
					target.Cancel(this, TargetCancel.Canceled);
			}
			else
			{
				base.OnSpellTarget(target, combatant);
			}
			
		}
	}
}