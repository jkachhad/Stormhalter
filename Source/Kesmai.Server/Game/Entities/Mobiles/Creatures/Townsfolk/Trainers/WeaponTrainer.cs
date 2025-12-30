using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Testura.Code.Extensions.Naming;

namespace Kesmai.Server.Game;

public partial class WeaponTrainer : TrainerEntity
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WeaponTrainer"/> class.
	/// </summary>
	public WeaponTrainer()
	{
	}
	
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		foreach (var skill in Skill.All)
		{
			var skillLevel = source.GetSkillLevel(skill);
			
			if (CanTrain(skill, out var entry) && skillLevel >= entry.Minimum && skillLevel < entry.Maximum)
				entries.Add(new TrainSkillInteraction(skill));
		}
		
		base.GetInteractions(source, entries);
	}
}

public class TrainSkillInteraction : InteractionEntry
{
	private Skill Skill { get; }

	public TrainSkillInteraction(Skill skill) : base(new LocalizationEntry(6500020, skill.Name.FirstLetterToUpperCase()), range: 0)
	{
		Skill = skill;
	}
	
	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not TrainerEntity trainer)
			return;

		if (!source.CanPerformAction)
		{
			source.SendMessage(Color.Red, "You are too busy to do that.");
			return;
		}

		if (source.Tranced && !source.IsSteering)
		{
			source.SendLocalizedMessage(Color.Red, 6300200); /* You can't do that while in a trance. */
			return;
		}

		if (!source.IsAlive || !trainer.CanSee(source))
		{
			trainer.SayTo(source, 6300235); /* I cannot serve you if I cannot see you. */
			return;
		}

		if (!trainer.CanTrain(source.Profession))
		{
			trainer.SayTo(source, 6300322); /* You are not of my profession! Get out! */
			return;
		}

		if (!trainer.CanTrain(Skill, out var entry))
		{
			trainer.SayTo(source, 6300239); /* I cannot train you in that discipline. */
			return;
		}

		var skillLevel = source.GetSkillLevel(Skill);

		if (skillLevel < entry.Minimum)
		{
			trainer.SayTo(source, 6300240); /* Come back when you have become more skilled. */
			return;
		}

		if (skillLevel > entry.Maximum)
		{
			trainer.SayTo(source, 6300241); /* I can teach you no more. */
			return;
		}

		if (source.Profession != Profession.MartialArtist && Skill == Skill.Hand && skillLevel >= 6.0)
		{
			trainer.SayTo(source, 6300242); /* You can learn more only by practicing. */
			return;
		}

		if (trainer.AtCounter(source, out var counter))
		{
			var segment = trainer.Segment;
			var gold = segment.GetItemsAt(counter).OfType<Gold>().ToList();

			if (gold.Any())
			{
				var trainAmount = gold.Sum(g => g.Amount);
				var canTrainNext = source.GetSkillLevel(Skill) < entry.Maximum;

				var (currentTraining, currentCost,
					nextTraining, nextCost) = source.CalculateTraining(Skill);

				if (currentTraining < 1 && nextTraining < 1)
				{
					trainer.SayTo(source, 6300244); /* You must practice more before I can train you again. */
				}
				else if (currentTraining < 1 && !canTrainNext)
				{
					trainer.SayTo(source, 6300241); /* I can teach you no more. */
				}
				else
				{
					var currentTrained = (long)(trainAmount / currentCost);
					var nextTrained = 0L;

					if (currentTrained >= currentTraining)
						currentTrained = currentTraining;

					var purchase = currentTrained * currentCost;

					if (canTrainNext && trainAmount > purchase && nextTraining > 0)
					{
						nextTrained = (long)(trainAmount - purchase) / nextCost;

						if (nextTrained >= nextTraining)
							nextTrained = nextTraining;

						purchase += nextTrained * nextCost;
					}

					if (!trainer.ConsumeFromLocation<Gold>(counter, (uint)purchase))
						return;

					var trained = currentTrained + nextTrained;

					source.Train(Skill, trained);
					source.AwardExperience(purchase);

					trainer.SayTo(source, 6300245, Skill.Name);
				}
			}
			else
			{
				if (trainer.Counters.Any())
					trainer.SayTo(source, 6300246); /* Please put some coins on the counter. */
				else
					trainer.SayTo(source, 6300247); /* Please put some coins on the ground. */
			}
		}
		else
		{
			if (trainer.Counters.Any())
				trainer.SayTo(source, 6300236); /* Please step up to a counter. */
			else
				trainer.SayTo(source, 6300237); /* Please stand closer to me. */
		}
	}
}

public class CritiqueSkillInteraction : InteractionEntry
{
	private Skill Skill { get; }

	public CritiqueSkillInteraction(Skill skill) : base(new LocalizationEntry(6500021, skill.Name.FirstLetterToUpperCase()), range: 0)
	{
		Skill = skill;
	}

	public override void OnClick(PlayerEntity source, WorldEntity target)
	{
		if (source is null || target is not TrainerEntity trainer)
			return;

		if (!trainer.CanTrain(source.Profession))
		{
			trainer.SayTo(source, 6300322); /* You are not of my profession! Get out! */
			return;
		}

		if (!trainer.CanTrain(Skill, out var _))
		{
			trainer.SayTo(source, 6300248); /* I am not qualified to critique that skill. */
			return;
		}

		var skillAchieved = source.Skills.GetAchievedLevel(Skill);
		var skillAchievedLevel = (int)skillAchieved;
		var skillPercent = (skillAchieved - skillAchievedLevel) * 100;
		var skillAchievedTitle = Skill.GetTitle(source, skillAchievedLevel);

		var skillCurrent = source.Skills.GetCurrentLevel(Skill);
		var skillCurrentTitle = Skill.GetTitle(source, (int)skillCurrent);

		var entries = new List<LocalizationEntry>();

		if (skillPercent > 0)
			entries.Add(new LocalizationEntry(6300249, $"{(int)skillPercent}", skillCurrentTitle)); /* You are {0:#0} percent above the level of {1}. */
		else if (skillAchievedLevel > 0)
			entries.Add(new LocalizationEntry(6300250, skillCurrentTitle)); /* You have achieved the level of {0}. */
		else
			entries.Add(new LocalizationEntry(6300251, skillCurrentTitle)); /* You are {0}. */

		if (skillCurrent < skillAchieved)
			entries.Add(new LocalizationEntry(6300252, skillAchievedTitle)); /* You are below your potential level of {0}. */

		trainer.SayTo(source, entries.ToArray());
	}
}
