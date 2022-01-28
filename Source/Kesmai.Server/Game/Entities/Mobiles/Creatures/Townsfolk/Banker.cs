using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game
{
	public partial class Banker : VendorEntity
	{
		private static Regex _deposit = new Regex(@"^deposit(\s?[\d]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex _withdraw = new Regex(@"^withdraw\s([\d]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		private static Regex _showBalance = new Regex(@"^show\sbalance$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
		public Banker()
		{
		}

		public override void HandleOrder(OrderEventArgs args)
		{
			base.HandleOrder(args);

			if (args.Handled)
				return;
			
			var source = args.Source;
			var order = args.Order;
			
			/* example: deposit */
			/* example: deposit (amount) */
			if (_deposit.TryGetMatch(order, out var depositMatch))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					var segment = Segment;
					var gold = segment.GetItemsAt(counter).OfType<Gold>().ToList();

					if (gold.Any())
					{
						var depositAmount = (uint)gold.Sum(g => g.Amount);

						if (depositMatch.Groups[1].Success && long.TryParse(depositMatch.Groups[1].Value, out var requestedAmount))
						{
							if (requestedAmount <= 0 || requestedAmount > UInt32.MaxValue || requestedAmount > depositAmount)
							{
								SayTo(source, 6300243); /* Are you trying to be funny? */
								return;
							}

							depositAmount = (uint)requestedAmount;
						}

						if (!ConsumeFromLocation<Gold>(counter, depositAmount))
							return;

						source.Gold += depositAmount;

						SayTo(source, 6300256, source.Gold.ToString()); /* Thank you. Your balance is now {0} coins. */
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
			
			/* example: withdraw (amount) */
			if (_withdraw.TryGetMatch(order, out var withdrawMatch))
			{
				args.Handled = true;
				
				if (AtCounter(source, out var counter))
				{
					if (withdrawMatch.Groups[1].Success && long.TryParse(withdrawMatch.Groups[1].Value, out var requestedAmount))
					{
						if (requestedAmount <= 0 || requestedAmount > UInt32.MaxValue)
						{
							SayTo(source, 6300243); /* Are you trying to be funny? */
						}
						else if (requestedAmount <= source.Gold)
						{
							source.Gold -= (uint)requestedAmount;

							var gold = new Gold()
							{
								Amount = (uint)requestedAmount
							};
							gold.Move(counter, true, Segment);
						}
						else
						{
							SayTo(source, 6300258); /* You don't have that much in your account. */
						}
					}
					else
					{
						SayTo(source, 6300257); /* Would you tell me the amount? */
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
			
			/* example: show balance */
			if (_showBalance.TryGetMatch(order, out var showBalanceMatch))
			{
				args.Handled = true;
				
				var balance = source.Gold;

				if (balance > 0)
					SayTo(source, 6300260, balance.ToString()); /* Your account balance is {0} coins. */
				else
					SayTo(source, 6300259); /* Your account is empty. */

				return;
			}
		}
	}
}