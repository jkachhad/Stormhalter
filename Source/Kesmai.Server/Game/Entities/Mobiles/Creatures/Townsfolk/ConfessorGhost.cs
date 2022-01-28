using System;
using System.IO;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public partial class ConfessorGhost : VendorEntity
	{
		public ConfessorGhost()
		{
			Name = "ghost";
			Body = 66;

			Health = MaxHealth = 50;
			
			IsInvulnerable = true;
		}

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;

			var source = args.Source;
			var order = args.Order;
			
			if (order.Matches("forgive me", true))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var silverDagger = Segment.GetItemsAt(Location).OfType<SilverDagger>();

					if (silverDagger.Any())
					{
						if (ConsumeFromLocation<SilverDagger>(counter, 1))
						{
							source.Alignment = default(Alignment);
							SayTo(source, 6300060); //A strange feeling comes over you.
						}
					}
					else
					{
						SayTo(source, 6300243); //Are you trying to be funny? 
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

		public void Forgive(MobileEntity entity)
		{
			
		}

		/* Prevent ghosts from taking damage. */
		public override bool AllowDamageFrom(ItemEntity item) => false;
		public override bool AllowDamageFrom(Spell spell) => false;
	}
}