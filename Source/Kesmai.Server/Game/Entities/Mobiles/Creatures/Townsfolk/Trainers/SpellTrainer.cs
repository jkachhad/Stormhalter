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

		if (CanTrain(skill, out var entry) && skillLevel >= entry.Minimum && skillLevel < entry.Maximum)
		{
			entries.Add(new CritiqueSkillInteraction(skill));
			entries.Add(new TrainSkillInteraction(skill));

			entries.Add(InteractionSeparator.Instance);
		}

		if (CanTrain(source.Profession))
		{
			var profession = source.Profession;

			var spells = profession.GetSpells();
			var available = spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();

			foreach (var spell in available)
				entries.Add(new TeachSpellInteraction(spell));
			
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
				
			if (AtCounter(source, out var counter))
			{
				var skillLevel = source.GetSkillLevel(Skill.Magic);
				var profession = source.Profession;

				var spells = profession.GetSpells();
				var available = spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();

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
				if (_counters.Any())
					SayTo(source, 6300236); /* Please step up to a counter. */
				else
					SayTo(source, 6300237); /* Please stand closer to me. */
			}

			return;
		}

		/* example: teach (spell name, index) */
		if (_teach.TryGetMatch(order, out var teachMatch))
		{
			args.Handled = true;
				
			if (AtCounter(source, out var counter))
			{
				if (teachMatch.Groups[1].Success)
				{
					var skillLevel = source.GetSkillLevel(Skill.Magic);
					var profession = source.Profession;

					var spells = profession.GetSpells();
					var available = spells.Where(st => st.SkillLevel <= skillLevel && !st.IsTaught(source)).ToList();

					if (available.Any())
					{
						/* Try to find the spell by name if index does not resolve. */
						if (!int.TryParse(teachMatch.Groups[1].Value, out var index))
						{
							var spellName = teachMatch.Groups[1].Value;
							var spell = available.FirstOrDefault(sp => sp.RespondsTo(spellName));

							if (spell != null)
								index = available.IndexOf(spell) + 1;
						}

						if (index > 0 && index <= available.Count)
						{
							var spell = available[index - 1];
							var gold = Segment.GetItemsAt(counter).OfType<Gold>().ToList();

							if (gold.Any())
							{
								var totalGold = gold.Sum(g => g.Amount);
								var spellCost = (uint)spell.Cost;

								if (totalGold >= spellCost && ConsumeFromLocation<Gold>(counter, spellCost))
								{
									foreach (var spellType in spell.Spells)
										source.Spells.Learn(spellType);

									SayTo(source, 6300325, spell.Name); /* You have learned the {0} spell. */
								}
								else
								{
									SayTo(source, 6300243); /* Are you trying to be funny? */
								}
							}
							else
							{
								if (_counters.Any())
									SayTo(source, 6300246); /* Please put some coins on the counter. */
								else
									SayTo(source, 6300247); /* Please put some coins on the ground. */
							}
						}
						else
						{
							SayTo(source, 6300321); /* That spell is not yet available to you. */
						}
					}
					else
					{
						SayTo(source, 6300323); /* There are no spells I can teach you. */
					}
				}
				else
				{
					SayTo(source, 6300324); /* I don't know what spell you mean. */
				}
			}
			else
			{
				if (_counters.Any())
					SayTo(source, 6300236); /* Please step up to a counter. */
				else
					SayTo(source, 6300237); /* Please stand closer to me. */
			}

			return;
		}
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