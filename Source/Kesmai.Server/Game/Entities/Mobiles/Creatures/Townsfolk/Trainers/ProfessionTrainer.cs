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

	private class SellBookInteraction : SellItemInteraction<Gold>
	{
		public SellBookInteraction() : base("Book")
		{
		}

		protected override uint GetCost(PlayerEntity player) 
			=> (uint)(((int)player.GetSkillLevel(Skill.Magic) + 1) * 100);

		protected override ItemEntity CreateItem(PlayerEntity player, VendorEntity vendor) 
			=> new Spellbook(player);
	}
}
