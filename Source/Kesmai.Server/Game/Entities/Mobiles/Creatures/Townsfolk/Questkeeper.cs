using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Kesmai.Server.Game
{
	public abstract partial class Questkeeper : VendorEntity
	{
		private static Regex _teach = new Regex(@"^teach\s?([\w]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		protected Questkeeper()
		{
		}

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;
			var order = args.Order;
			
			if (_teach.TryGetMatch(order, out var teachMatch))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					OnTeach(source);
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
			
			if (order.Matches("yes", true))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					OnAccept(source);
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
			
			if (order.Matches("no", true))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					OnDecline(source);
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

		protected virtual void OnTeach(PlayerEntity player)
		{
		}

		protected virtual void OnAccept(PlayerEntity player)
		{
		}

		protected virtual void OnDecline(PlayerEntity player)
		{
		}
	}
}