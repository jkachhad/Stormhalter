using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Game.Items.Magical;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract partial class ProfessionTrainer : TrainerEntity
{
	public abstract Profession Profession { get; }
		
	public ProfessionTrainer()
	{
	}
		
	public override void HandleOrder(OrderEventArgs args)
	{
		base.HandleOrder(args);

		if (args.Handled)
			return;
			
		var source = args.Source;
		var order = args.Order;
			
		/* example: sell book */
		if (_sell.TryGetMatch(order, out var sellMatch) && sellMatch.Groups[1].Value.Matches("book", true))
		{
			args.Handled = true;
				
			if (AtCounter(source, out var counter))
			{
				var skillLevel = (int)source.GetSkillLevel(Skill.Magic) + 1;
				var cost = (uint)(skillLevel * 100);

				var segment = Segment;
				var gold = segment.GetItemsAt(counter).OfType<Gold>().ToList();

				if (gold.Any())
				{
					if (ConsumeFromLocation<Gold>(counter, cost))
					{
						new Spellbook(source)
							.Move(counter, true, segment);

						SayTo(source, 6300340); /* Don't go losing it again. */
					}
					else
					{
						SayTo(source, 6300339, cost); /* For you, I will sell one for {0} coins. */
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
				if (_counters.Any())
					SayTo(source, 6300236); /* Please step up to a counter. */
				else
					SayTo(source, 6300237); /* Please stand closer to me. */
			}
				
			return;
		}
	}

	public override bool CanTrain(Profession profession)
	{
		if (profession != Profession)
			return false;

		return base.CanTrain(profession);
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (CanTrain(source.Profession))
		{
			entries.Add(new SellBookInteraction());
			entries.Add(InteractionSeparator.Instance);
		}
		
		base.GetInteractions(source, entries);
	}

	private class SellBookInteraction : InteractionEntry
	{
		public SellBookInteraction() : base(new LocalizationEntry(6500010, "Book"))
		{
		}

		public override void OnClick(PlayerEntity source, WorldEntity target)
		{
			if (source is null || target is not ProfessionTrainer trainer)
				return;

			if (!source.CanPerformAction)
				return;

			if (source.Tranced && !source.IsSteering)
			{
				source.SendLocalizedMessage(System.Drawing.Color.Red, 6300200); /* You can't do that while in a trance. */
				return;
			}

			var segment = trainer.Segment;

			if (trainer.AtCounter(source, out var counter))
			{
				var skillLevel = (int)source.GetSkillLevel(Skill.Magic) + 1;
				var cost = (uint)(skillLevel * 100);
				
				var gold = segment.GetItemsAt(counter).OfType<Gold>().ToList();

				if (gold.Any())
				{
					if (trainer.ConsumeFromLocation<Gold>(counter, cost))
					{
						new Spellbook(source)
							.Move(counter, true, segment);

						trainer.SayTo(source, 6300340); /* Don't go losing it again. */
					}
					else
					{
						trainer.SayTo(source, 6300339, cost); /* For you, I will sell one for {0} coins. */
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
}