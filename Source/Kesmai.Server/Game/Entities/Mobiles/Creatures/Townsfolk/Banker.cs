using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;

namespace Kesmai.Server.Game;

public partial class Banker : VendorEntity
{
	public Banker()
	{
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(DepositAllInteraction.Instance);
		entries.Add(_depositAmount);
		entries.Add(_withdrawAmount);
		entries.Add(ShowBalanceInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	private static readonly VendorInputInteraction _depositAmount = new VendorInputInteraction("Deposit…",
		"How many coins do you want to deposit?",
		(player, vendor, counter, text) => ((Banker)vendor).Deposit(player, counter, UInt32.Parse(text)),
		InputKind.Number);

	private static readonly VendorInputInteraction _withdrawAmount = new VendorInputInteraction("Withdraw…",
		"How many coins do you want to withdraw?",
		(player, vendor, counter, text) => ((Banker)vendor).Withdraw(player, counter, UInt32.Parse(text)),
		InputKind.Number);

	private void Withdraw(PlayerEntity source, Point2D counter, uint amount)
	{
		if (amount > source.Gold)
		{
			SayTo(source, 6300258); /* You don't have that much in your account. */
			return;
		}

		source.Gold -= amount;

		new Gold()
		{
			Amount = amount
		}.Move(counter, true, Segment);
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
