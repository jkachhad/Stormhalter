using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Banker : VendorEntity
{
	/* Typed amounts stay as speech until the input dialog (Kesmai #361). */
	private static Regex _deposit = new Regex(@"^deposit\s([\d]+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
	private static Regex _withdraw = new Regex(@"^withdraw\s([\d]*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		
	public Banker()
	{
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(DepositAllInteraction.Instance);
		entries.Add(ShowBalanceInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	public override void HandleOrder(OrderEventArgs args)
	{
		base.HandleOrder(args);

		if (args.Handled)
			return;
			
		var source = args.Source;
		var order = args.Order;
			
		/* example: deposit (amount) */
		if (_deposit.TryGetMatch(order, out var depositMatch))
		{
			args.Handled = true;
				
			if (!AtCounter(source, out var counter))
			{
				this.SayNotAtCounter(source);
				return;
			}

			if (!long.TryParse(depositMatch.Groups[1].Value, out var requestedAmount) || requestedAmount <= 0 || requestedAmount > UInt32.MaxValue)
			{
				SayTo(source, 6300243); /* Are you trying to be funny? */
				return;
			}

			Deposit(source, counter, (uint)requestedAmount);
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
				this.SayNotAtCounter(source);
			}

			return;
		}
	}

	/// <summary>
	/// Deposits coins from the counter. With no amount, deposits all of them.
	/// </summary>
	private void Deposit(PlayerEntity source, Point2D counter, uint? amount = null)
	{
		var onCounter = this.CountOnCounter<Gold>(counter);

		if (onCounter is 0)
		{
			this.SayPutCoins(source);
			return;
		}

		var depositAmount = amount ?? onCounter;

		if (depositAmount > onCounter)
		{
			SayTo(source, 6300243); /* Are you trying to be funny? */
			return;
		}

		if (!this.ConsumeFromCounter<Gold>(counter, depositAmount))
			return;

		source.Gold += depositAmount;

		SayTo(source, 6300256, source.Gold.ToString("#,##0")); /* Thank you. Your balance is now {0} coins. */
	}

	private void ShowBalance(PlayerEntity source)
	{
		var balance = source.Gold;

		if (balance > 0)
			SayTo(source, 6300260, balance.ToString("#,##0")); /* Your account balance is {0} coins. */
		else
			SayTo(source, 6300259); /* Your account is empty. */
	}

	private sealed class DepositAllInteraction : VendorInteraction
	{
		public static readonly DepositAllInteraction Instance = new DepositAllInteraction();

		private DepositAllInteraction() : base("Deposit all")
		{
		}

		protected override void OnClick(PlayerEntity player, VendorEntity vendor, Point2D counter)
		{
			if (vendor is Banker banker)
				banker.Deposit(player, counter);
		}
	}

	private sealed class ShowBalanceInteraction : VendorInteraction
	{
		public static readonly ShowBalanceInteraction Instance = new ShowBalanceInteraction();

		private ShowBalanceInteraction() : base("Show balance")
		{
		}

		protected override void OnClick(PlayerEntity player, VendorEntity vendor, Point2D counter)
		{
			if (vendor is Banker banker)
				banker.ShowBalance(player);
		}
	}
}
