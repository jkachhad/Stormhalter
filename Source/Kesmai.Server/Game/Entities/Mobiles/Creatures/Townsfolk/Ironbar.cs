using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
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

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;
			var order = args.Order;
			
			/* example: craft broadsword */
			if (_craft.TryGetMatch(order, out var craftMatch) && craftMatch.Groups[1].Value.Matches("broadsword", true))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var items = Segment.GetItemsAt(Location).ToList();

					var ironOre = items.OfType<IronOre>();
					var yttril = items.OfType<Yttril>();

					if (ironOre.Any() && yttril.Any())
					{
						if (ConsumeFromLocation<IronOre>(counter, 1) && ConsumeFromLocation<Yttril>(counter, 1))
						{
							new BlackBroadsword().Move(counter, true, Segment);

							SayTo(source, 6300349); /* Wield this weapon with courage and honor. */
						}
					}
					else
					{
						SayTo(source, 6300348); /* Bring me iron and yttril and I will make you the weapon. */
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
	}
}