using System.IO;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game
{
	public partial class Salamander : AnimalEntity
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Salamander"/> class.
		/// </summary>
		public Salamander()
		{
			Name = "salamander";
			Body = 40;

			Alignment = Alignment.Chaotic;
		}

		/// <inheritdoc/>
		public override void OnSpawn()
		{
			base.OnSpawn();
			
			if (_brain != null)
				return;
			
			_brain = new CombatAI(this);
		}
		
		public override int GetNearbySound() => 17;
		public override int GetAttackSound() => 29;
		public override int GetDeathSound() => 41;
		
		public override Corpse GetCorpse()
		{
			var corpse = base.GetCorpse();
			
			if (corpse != null)
				corpse.CanBurn = false;

			return corpse;
		}
		
		public override ItemEntity OnCorpseTanned()
		{
			return new SalamanderScales();
		}
		
		public override void OnSpellTarget(Target target, MobileEntity combatant)
		{
			var spell = Spell;

			if (spell is FirewallSpell firewall)
				firewall.CastAt(combatant.Location, Direction.Cardinal.Random());

			base.OnSpellTarget(target, combatant);
		}
	}
}