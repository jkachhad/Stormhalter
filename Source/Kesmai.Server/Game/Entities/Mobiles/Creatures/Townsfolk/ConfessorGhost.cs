using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class ConfessorGhost : VendorEntity
{
	public ConfessorGhost()
	{
		Name = "ghost";
		Body = 66;

		Health = MaxHealth = 50;
			
		IsInvulnerable = true;
	}

	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		entries.Add(ForgiveInteraction.Instance);
		entries.Add(InteractionSeparator.Instance);

		base.GetInteractions(source, entries);
	}

	/// <summary>
	/// A silver dagger on the counter resets the player's alignment.
	/// </summary>
	private sealed class ForgiveInteraction : VendorInteraction
	{
		public static readonly ForgiveInteraction Instance = new ForgiveInteraction();

		private ForgiveInteraction() : base("Ask forgiveness")
		{
		}

		protected override void OnClick(PlayerEntity player, VendorEntity vendor, Point2D counter)
		{
			if (!vendor.ConsumeFromCounter<SilverDagger>(counter))
			{
				vendor.SayTo(player, 6300243); /* Are you trying to be funny? */
				return;
			}

			player.Alignment = default(Alignment);
			vendor.SayTo(player, 6300060); /* A strange feeling comes over you. */
		}
	}

	public void Forgive(MobileEntity entity)
	{
			
	}

	/* Prevent ghosts from taking damage. */
	public override bool AllowDamageFrom(ItemEntity item) => false;
	public override bool AllowDamageFrom(Spell spell) => false;
}