using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Game
{
	public partial class SummonedSalamander : Salamander
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SummonedSalamander"/> class.
		/// </summary>
		public SummonedSalamander(ICreatureSpell spell)
		{
			Summoned = true;
			
			Health = MaxHealth = 71;
			BaseDodge = 15;
			
			Movement = 2;

			Alignment = Alignment.Lawful;

			Attacks = new CreatureAttackCollection
			{
				{ new CreatureAttack(10, 7, 14, "The salamander claws at you"), 60 },
				{ new CreatureAttack(10, 9, 17, "The salamander hits you with its tail"), 40 },
			};

			Blocks = new CreatureBlockCollection
			{
				{ new CreatureBlock(6, "the armor") },
				{ new CreatureBlock(3, "a claw") },
				{ new CreatureBlock(1, "a tail") },
			};
			
			Spells = new CreatureSpellCollection()
			{
				{ new CreatureSpellEntry(spell, 100, TimeSpan.Zero )}
			};

			FireProtection = 100;
		}

		/// <inheritdoc/>
		protected override void OnLoad()
		{
			base.OnLoad();

			if (_brain != null)
				return;
			
			_brain = new CombatAI(this);
		}

		public override Corpse GetCorpse() => default(Corpse);
	}
}