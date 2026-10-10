using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class BLAlchemist : VendorEntity
{
	public BLAlchemist()
	{
		Name = "Alchemist";
		Body = 75;

		Health = MaxHealth = 50;
		BaseDodge = 10;

		Experience = 50;

		Movement = 0;
	}

	/* Same recipe as Ironbar: iron ore and yttril make a black broadsword. */
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(Ironbar.CraftBlackBroadswordInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}
}