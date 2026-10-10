using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

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

	/// <summary>
	/// The questkeeper response an entry triggers.
	/// </summary>
	public enum QuestResponse
	{
		Teach,
		Accept,
		Decline,
	}

	/// <summary>
	/// A stock entry that calls <see cref="OnTeach"/>, <see cref="OnAccept"/> or <see cref="OnDecline"/>
	/// with the quest's own title, after the standard vendor and counter checks.
	/// </summary>
	public sealed class QuestInteraction : VendorInteraction
	{
		private readonly QuestResponse _response;

		public QuestResponse Response => _response;

		public QuestInteraction(QuestResponse response, LocalizationEntry title, int range = -1) : base(title, range)
		{
			_response = response;
		}

		protected override bool CanUse(PlayerEntity player, VendorEntity vendor)
		{
			return player.IsAlive && vendor.CanSee(player);
		}

		protected override void OnCannotUse(PlayerEntity player, VendorEntity vendor)
		{
			vendor.SayTo(player, 6300235); /* I cannot serve you if I cannot see you. */
		}

		protected override void OnClick(PlayerEntity player, VendorEntity vendor, Point2D counter)
		{
			if (vendor is not Questkeeper keeper)
				return;

			switch (_response)
			{
				case QuestResponse.Teach: keeper.OnTeach(player); break;
				case QuestResponse.Accept: keeper.OnAccept(player); break;
				case QuestResponse.Decline: keeper.OnDecline(player); break;
			}
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