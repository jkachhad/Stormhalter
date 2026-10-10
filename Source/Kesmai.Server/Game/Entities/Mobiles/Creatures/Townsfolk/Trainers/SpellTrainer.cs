using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract partial class SpellTrainer : ProfessionTrainer
{
	public SpellTrainer()
	{
	}
	
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		var skill = Skill.Magic;
		var skillLevel = source.GetSkillLevel(skill);

		if (CanTrain(skill, out var entry) && CanTrain(source.Profession))
		{
			if (skillLevel >= entry.Minimum && skillLevel < entry.Maximum)
				entries.Add(new TrainSkillInteraction(skill));

			entries.Add(new CritiqueSkillInteraction(skill));
			entries.Add(new CritiqueTrainingInteraction(skill));

			entries.Add(InteractionSeparator.Instance);
		}

		if (CanTrain(source.Profession))
		{
			var profession = source.Profession;

			var spells = profession.GetSpells();
			var available = spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();

			if (available.Any())
			{
				var teach = new InteractionSubmenu("Teach spell");

				foreach (var spell in available)
					teach.Add(new TeachSpellInteraction(spell));

				entries.Add(teach);
			}
			
			entries.Add(InteractionSeparator.Instance);
		}

		base.GetInteractions(source, entries);
	}

	public class TeachSpellInteraction : VendorInteraction
	{
		private ProfessionSpell _spell;

		public TeachSpellInteraction(ProfessionSpell spell) : base(new LocalizationEntry(6500012, spell.Name, spell.Cost.ToString()))
		{
			_spell = spell;
		}

		protected override bool CanUse(PlayerEntity player, VendorEntity vendor)
		{
			return vendor is SpellTrainer trainer && trainer.CanTrain(player.Profession);
		}

		protected override void OnClick(PlayerEntity source, VendorEntity vendor, Point2D counter)
		{
			var trainer = (SpellTrainer)vendor;
			var skillLevel = source.GetSkillLevel(Skill.Magic);
			var profession = source.Profession;

			var spells = profession.GetSpells();
			var available = spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();

			if (!available.Any())
			{
				trainer.SayTo(source, 6300323); /* There are no spells I can teach you. */
				return;
			}
			
			if (!available.Contains(_spell))
			{
				trainer.SayTo(source, 6300321); /* That spell is not yet available to you. */
				return;
			}

			var spellCost = (uint)_spell.Cost;

			if (trainer.CountOnCounter<Gold>(counter) is 0)
			{
				trainer.SayPutCoins(source);
				return;
			}

			if (!trainer.ConsumeFromCounter<Gold>(counter, spellCost))
			{
				trainer.SayTo(source, 6300243); /* Are you trying to be funny? */
				return;
			}

			foreach (var spellType in _spell.Spells)
				source.Spells.Learn(spellType);

			trainer.SayTo(source, 6300325, _spell.Name); /* You have learned the {0} spell. */
		}
	}
}