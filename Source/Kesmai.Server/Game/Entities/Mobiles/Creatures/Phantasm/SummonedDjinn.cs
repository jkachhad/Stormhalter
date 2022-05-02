using System;
using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class SummonedDjinn : Djinn
	{
		public override bool CanOrderFollow => true;
		public override bool CanOrderAttack => true;
		public override bool CanOrderCarry => true;

		public SummonedDjinn(ICreatureSpell spell)
		{
			Summoned = true;
			
			Health = MaxHealth = 300;
			BaseDodge = 25;
			Mana = MaxMana = 24;
			IceProtection = 90;
			MagicProtection = 15;

			Movement = 3;
			
			Attacks = new CreatureAttackCollection
			{
				{ new CreatureBasicAttack(14) },
			};

			Blocks = new CreatureBlockCollection
			{
				new CreatureBlock(12, "a hand"),
			};
			
			Spells = new CreatureSpellCollection()
			{
				{ new CreatureSpellEntry(spell, 100, TimeSpan.Zero ) }
			};
			
			AddStatus(new NightVisionStatus(this));

			CanFly = true;
		}
		
		protected override void OnLoad()
		{
			base.OnLoad();
			
			_brain = new CombatAI(this);
		}

		public override bool AllowDamageFrom(Spell spell)
		{
			if (Spells.Any((e => e.Spell.SpellType == spell.GetType()), out CreatureSpellEntry entry))
				return false;

			return true;
		}
	}
}