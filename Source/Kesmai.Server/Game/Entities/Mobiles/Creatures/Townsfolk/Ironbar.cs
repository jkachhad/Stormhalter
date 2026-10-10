using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public partial class Ironbar : VendorEntity
{
	public Ironbar()
	{
		Name = "Ironbar";
		Body = 98;

		Health = MaxHealth = 50;
		BaseDodge = 10;

		Experience = 50;

		Movement = 0;
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(CraftBlackBroadswordInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	internal sealed class CraftBlackBroadswordInteraction : CraftItemInteraction
	{
		public static readonly CraftBlackBroadswordInteraction Instance = new CraftBlackBroadswordInteraction();
		
		public CraftBlackBroadswordInteraction() : base(new LocalizationEntry(6500014, "Black Broadsword"), range: 0) /* craft: {0} */
		{
		}

		protected override int GetCost(PlayerEntity player) => 0;
	
		protected override IEnumerable<(Type Resource, int Amount)> GetResources(PlayerEntity player)
		{
			yield return (typeof(IronOre), 1);
			yield return (typeof(Yttril), 1);
		}

		protected override IEnumerable<ItemEntity> GetCraftedItems(PlayerEntity player)
		{
			yield return new BlackBroadsword();
		}

		protected override void OnCrafted(PlayerEntity player, VendorEntity vendor)
		{
			vendor.SayTo(player, 6300349); /* Wield this weapon with courage and honor. */
			vendor.EmitSound(10005, 3, 6);
		}

		protected override void OnMissingResources(PlayerEntity player, VendorEntity vendor)
		{
			vendor.SayTo(player, 6300348); /* Bring me iron and yttril and I will make you the weapon. */
		}
	}
}