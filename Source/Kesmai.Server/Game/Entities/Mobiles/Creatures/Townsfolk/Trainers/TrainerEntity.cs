using Kesmai.Server.Items;
using Kesmai.Server.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game.Items.Magical;
using Kesmai.Server.Miscellaneous;

namespace Kesmai.Server.Game
{
	public abstract partial class TrainerEntity : VendorEntity
	{
		private static Regex _train = new Regex(@"^train(\s?[\d]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex _critiqueSkill = new Regex(@"^critique\s(\w.*?)\sskill?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex _critiqueTraining = new Regex(@"^critique\s(\w.*?)\straining?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		protected Dictionary<Skill, TrainingEntry> _training;

		/// <inheritdoc />
		public override bool CanPerformMovement => false;

		/// <inheritdoc />
		protected TrainerEntity()
		{
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_training = new Dictionary<Skill, TrainingEntry>();
		}

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;
			var order = args.Order;
			
			if (!CanTrain(source.Profession))
			{
				SayTo(source, 6300322); /* You are not of my profession! Get out! */

				args.Handled = true;
				return;
			}
			
			/* example: train */
			/* example: train (amount) */
			if (_train.TryGetMatch(order, out var trainMatch) && CanTrain(source, out var skill, out var entry))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var segment = Segment;
					var gold = segment.GetItemsAt(counter).OfType<Gold>().ToList();

					if (gold.Any())
					{
						var trainAmount = gold.Sum(g => g.Amount);
						var canTrainNext = source.GetSkillLevel(skill) < entry.Maximum;

						var (currentTraining, currentCost,
							nextTraining, nextCost) = source.CalculateTraining(skill);

						if (trainMatch.Groups[1].Success && long.TryParse(trainMatch.Groups[1].Value, out var requestedAmount))
						{
							if (requestedAmount <= 0 || requestedAmount > UInt32.MaxValue || requestedAmount > trainAmount)
							{
								SayTo(source, 6300243); /* Are you trying to be funny? */
								return;
							}

							trainAmount = requestedAmount;
						}

						if (currentTraining < 1 && nextTraining < 1)
						{
							SayTo(source, 6300244); /* You must practice more before I can train you again. */
						}
						else if (currentTraining < 1 && !canTrainNext)
						{
							SayTo(source, 6300241); /* I can teach you no more. */
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

							if (!ConsumeFromLocation<Gold>(counter, (uint)purchase))
								return;
			
							var trained = currentTrained + nextTrained;
							
							source.Train(skill, trained);
							source.AwardExperience(purchase);

							SayTo(source, 6300245, skill.Name);
						}
					}
					else
					{
						if (Counters.Any())
							SayTo(source, 6300246); /* Please put some coins on the counter. */
						else
							SayTo(source, 6300247); /* Please put some coins on the ground. */
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

			/* example: critique (skill) skill */
			if (_critiqueSkill.TryGetMatch(order, out var critiqueSkillMatch))
			{
				args.Handled = true;
				
				var skillName = critiqueSkillMatch.Groups[1].Value;
				var critiqueSkill = Skill.All.FirstOrDefault(s => s.Name.Matches(skillName, true));

				if (critiqueSkill != null && CanTrain(critiqueSkill, out var _))
				{
					var skillAchieved = source.Skills.GetAchievedLevel(critiqueSkill);
					var skillAchievedLevel = (int)skillAchieved;
					var skillPercent = (skillAchieved - skillAchievedLevel) * 100;
					var skillAchievedTitle = critiqueSkill.GetTitle(source, skillAchievedLevel);

					var skillCurrent = source.Skills.GetCurrentLevel(critiqueSkill);
					var skillCurrentTitle = critiqueSkill.GetTitle(source, (int)skillCurrent);

					var entries = new List<LocalizationEntry>();

					if (skillPercent > 0)
						entries.Add(new LocalizationEntry(6300249, $"{(int)skillPercent}", skillCurrentTitle)); /* You are {0:#0} percent above the level of {1}. */
					else if (skillAchievedLevel > 0)
						entries.Add(new LocalizationEntry(6300250, skillCurrentTitle)); /* You have achieved the level of {0}. */
					else
						entries.Add(new LocalizationEntry(6300251, skillCurrentTitle)); /* You are {0}. */

					if (skillCurrent < skillAchieved)
						entries.Add(new LocalizationEntry(6300252, skillAchievedTitle)); /* You are below your potential level of {0}. */

					SayTo(source, entries.ToArray());
				}
				else
				{
					SayTo(source, 6300248); /* I am not qualified to critique that skill. */
				}

				return;
			}
			
			/* example: critique (skill) training */
			if (_critiqueTraining.TryGetMatch(order, out var critiqueTrainingMatch))
			{
				args.Handled = true;

				var skillName = critiqueTrainingMatch.Groups[1].Value;
				var critiqueSkill = Skill.All.FirstOrDefault(s => s.Name.Matches(skillName, true));

				if (critiqueSkill != null && CanTrain(critiqueSkill, out var _))
				{
					var skillLevel = source.Skills.GetAchievedLevel(critiqueSkill);
					var skillLevelNext = Math.Min((int)skillLevel + 1, 19);
					
					var skillExperience = source.Skills.GetAchievedExperience(critiqueSkill);

					var trained = source.Skills.GetTraining(critiqueSkill);
					var trainedSkill = skillExperience + (trained * 2);
					var trainedLevel = Skill.GetLevel(trainedSkill);
					
					var skillCurrentTitle = critiqueSkill.GetTitle(source, (int)skillLevel);
					var skillNextTitle = critiqueSkill.GetTitle(source, (int)skillLevelNext);

					if (trainedLevel > skillLevelNext)
					{
						SayTo(source, 6300371, skillCurrentTitle,
							((int)((trainedLevel - skillLevelNext) * 100)).ToString(), skillNextTitle); /* You are fully trained in the level of {0}. You are trained to {1}% in the level of {2}. */
					}
					else if (trainedLevel < skillLevelNext)
					{
						SayTo(source, 6300370, 
							((int)((trainedLevel - Math.Truncate(trainedLevel)) * 100)).ToString(), skillCurrentTitle); /* You are trained to {0}% in the level of {1}. */
					}
					else
					{
						SayTo(source, 6300369, skillCurrentTitle); /* You are fully trained in the level of {0}. */
					}
				}
				else
				{
					SayTo(source, 6300248); /* I am not qualified to critique that skill. */
				}
				
				return;
			}
		}

		public virtual bool CanTrain(PlayerEntity mobile, out Skill skill, out TrainingEntry entry)
		{
			entry = default(TrainingEntry);

			skill = Skill.Hand;

			if (mobile.RightHand is Weapon weapon)
			{
				skill = weapon.Skill;
			}
			else if (mobile.RightHand is Spellbook spellbook)
			{
				if (spellbook.Owner != mobile)
				{
					SayTo(mobile, 6300254); /* That spell book does not belong to you!*/
					return false;
				}
				
				skill = Skill.Magic;
			}	
			else if (mobile.RightHand != default(ItemEntity))
			{
				SayTo(mobile, 6300238); /* I don't know how to train you with that item. */
				return false;
			}

			if (CanTrain(skill, out entry) && CanTrain(mobile.Profession))
			{
				var skillLevel = mobile.GetSkillLevel(skill);

				if (skillLevel < entry.Minimum)
					SayTo(mobile, 6300240); /* Come back when you have become more skilled. */
				else if (skillLevel > entry.Maximum)
					SayTo(mobile, 6300241); /* I can teach you no more. */
				else if (mobile.Profession != Profession.MartialArtist && skill == Skill.Hand && skillLevel >= 6.0)
					SayTo(mobile, 6300242); /* You can learn more only by practicing. */
				else
					return true;
			}
			else
			{
				SayTo(mobile, 6300239); /* I cannot train you in that discipline. */
			}

			return false;
		}

		/// <summary>
		/// Determines whether this instance can train the specified skill.
		/// </summary>
		private bool CanTrain(Skill skill, out TrainingEntry entry)
		{
			return _training.TryGetValue(skill, out entry);
		}

		public virtual bool CanTrain(Profession profession)
		{
			return true;
		}

		[WorldForge]
		public void SetTraining(Skill skill, double minimum, double maximum)
		{
			if (!_training.ContainsKey(skill))
				_training.Add(skill, new TrainingEntry(skill, minimum, maximum));
		}
	}

	public partial class TrainingEntry
	{
		public Skill Skill { get; set; }

		public double Minimum { get; set; }
		public double Maximum { get; set; }

		public TrainingEntry(Skill skill, double minimum, double maximum)
		{
			Skill = skill;
			Minimum = minimum;
			Maximum = maximum;
		}
	}
}
