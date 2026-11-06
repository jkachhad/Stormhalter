using System;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Network;

namespace Kesmai.Server.Game;

public abstract partial class Questkeeper : VendorEntity
{
	protected Questkeeper()
	{
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
	/// An entry that calls <see cref="OnTeach"/>, <see cref="OnAccept"/> or <see cref="OnDecline"/>
	/// after the standard vendor and counter checks. Each questkeeper lists, in its own
	/// <see cref="WorldEntity.GetInteractions"/>, only the responses that apply to the player's quest stage.
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