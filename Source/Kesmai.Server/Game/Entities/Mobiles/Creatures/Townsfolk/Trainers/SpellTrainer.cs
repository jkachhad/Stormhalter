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
	private static Regex _teach = new Regex(@"^teach\s([\w\s\/]*?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
			var available = GetAvailableSpells(source);

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

	public override void HandleOrder(OrderEventArgs args)
	{
		base.HandleOrder(args);

		if (args.Handled)
			return;

		var source = args.Source;
		var order = args.Order;

		/* example: show spells */
		if (order.Matches("show spells", true))
		{
			args.Handled = true;

			if (AtCounter(source, out _))
			{
				var available = GetAvailableSpells(source);

				if (available.Any())
				{
					for (var i = 0; i < available.Count; i++)
						SayTo(source, $"{i + 1}. {available[i].Name} - {available[i].Cost} coins.");
				}
				else
				{
					SayTo(source, 6300323); /* There are no spells I can teach you. */
				}
			}
			else
			{
				this.SayNotAtCounter(source);
			}

			return;
		}

		/* example: teach (spell name, index) */
		if (_teach.TryGetMatch(order, out var teachMatch))
		{
			args.Handled = true;

			if (AtCounter(source, out var counter))
			{
				var available = GetAvailableSpells(source);
				var spell = default(ProfessionSpell);

				/* Try to find the spell by name if index does not resolve. */
				if (int.TryParse(teachMatch.Groups[1].Value, out var index))
				{
					if (index > 0 && index <= available.Count)
						spell = available[index - 1];
				}
				else
				{
					var spellName = teachMatch.Groups[1].Value;

					spell = available.FirstOrDefault(sp => sp.RespondsTo(spellName));
				}

				TeachSpell(source, counter, spell);
			}
			else
			{
				this.SayNotAtCounter(source);
			}

			return;
		}
	}

	/// <summary>
	/// Gets the spells of the player's profession they have the skill for and haven't learned.
	/// </summary>
	private static List<ProfessionSpell> GetAvailableSpells(PlayerEntity source)
	{
		var skillLevel = source.GetSkillLevel(Skill.Magic);
		var spells = source.Profession.GetSpells();

		return spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();
	}

	/// <summary>
	/// Teaches the spell for the gold on the counter.
	/// </summary>
	private void TeachSpell(PlayerEntity source, Point2D counter, ProfessionSpell spell)
	{
		var available = GetAvailableSpells(source);

		if (!available.Any())
		{
			SayTo(source, 6300323); /* There are no spells I can teach you. */
			return;
		}

		if (spell is null || !available.Contains(spell))
		{
			SayTo(source, 6300321); /* That spell is not yet available to you. */
			return;
		}

		if (this.CountOnCounter<Gold>(counter) is 0)
		{
			this.SayPutCoins(source);
			return;
		}

		if (!this.ConsumeFromCounter<Gold>(counter, (uint)spell.Cost))
		{
			SayTo(source, 6300243); /* Are you trying to be funny? */
			return;
		}

		foreach (var spellType in spell.Spells)
			source.Spells.Learn(spellType);

		SayTo(source, 6300325, spell.Name); /* You have learned the {0} spell. */
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
			if (vendor is SpellTrainer trainer)
				trainer.TeachSpell(source, counter, _spell);
		}
	}
}