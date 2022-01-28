using System;
using System.IO;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class SummonedEfreet : Efreet
	{
		public override bool CanOrderFollow => true;
		public override bool CanOrderAttack => true;
		public override bool CanOrderCarry => true;
		
		public SummonedEfreet(ICreatureSpell spell)
		{
			Summoned = true;
			
			Health = MaxHealth = 200;
			BaseDodge = 19;
			Mana = MaxMana = 40;
			FireProtection = 100;
			Movement = 3;
			
			Attacks = new CreatureAttackCollection
			{
				{ new CreatureBasicAttack(14) },
			};

			Blocks = new CreatureBlockCollection
			{
				new CreatureBlock(14, "a hand"),
			};

			Spells = new CreatureSpellCollection()
			{
				{ new CreatureSpellEntry(spell, 100, TimeSpan.Zero )}
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