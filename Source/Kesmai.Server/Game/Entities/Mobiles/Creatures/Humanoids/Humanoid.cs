using System;
using System.Collections.Generic;
using Kesmai.Server.Engines.Interactions;
using Kesmai.Server.Items;
using Kesmai.Server.Network;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public partial class Humanoid : CreatureEntity
{
	private static readonly TimeSpan GossipCooldown = TimeSpan.FromMinutes(10);
	
	private record GossipConversation(DateTime NextGossipTime)
	{
		public DateTime NextGossipTime { get; set; } = NextGossipTime;

		public bool IsAvailable => (Server.Now >= NextGossipTime);
	}
	
	private Dictionary<PlayerEntity, GossipConversation> _gossips = new Dictionary<PlayerEntity, GossipConversation>();
	
	/// <summary>
	/// Gets or sets the gossip interactions for this humanoid.
	/// </summary>
	/// <remarks>
	/// Gossiping is typically initiated by a player using an interaction. The reaction from the humanoid
	/// can vary based on the implementation of this action. This could involve providing information,
	/// giving quests, or other behaviors.
	///
	/// Try to keep the number of gossip interactions reasonable to avoid overwhelming players with options. Furthermore,
	/// implement interactions as static instances where possible to reduce memory overhead. For example, if multiple humanoids
	/// share the same gossip interaction, they should reference the same instance rather than creating new one.
	/// </remarks>
	public List<GossipInteraction> Gossips { get; set; }
	
	public Humanoid()
	{
		Alignment = Alignment.Chaotic;
		CanSwim = true;

		AddStatus(new BreatheWaterStatus(this));
	}
		
	public override int GetDeathSound() => (IsFemale ? 83 : 79);
	public override int GetWarmSound() => (IsFemale ? 84 : 80);

	public override ItemEntity OnCorpseTanned()
	{
		return new LeatherJacket();
	}

	public override void OnSpawn()
	{
		base.OnSpawn();

		if (_brain != null)
			return;
			
		if (RightHand is ProjectileWeapon)
			_brain = new RangedAI(this);
		else
			_brain = new CombatAI(this);
	}
	
	/// <inheritdoc/>
	public override void GetInteractions(PlayerEntity source, List<InteractionEntry> entries)
	{
		if (Gossips != null && CanGossip(source))
		{
			entries.AddRange(Gossips);
			entries.Add(InteractionSeparator.Instance);
		}
		
		base.GetInteractions(source, entries);
	}
	
	protected virtual bool CanGossip(PlayerEntity source)
	{
		// Check if there is an existing gossip conversation state for this player.
		if (_gossips.TryGetValue(source, out var gossip))
			return gossip.IsAvailable;
		
		// By default, all players can gossip with humanoids that have gossip interactions.
		return true;
	}

	/// <summary>
	/// Handles a gossip request from the specified yapper.
	/// </summary>
	/// <param name="yapper">The player looking to gossip with this instance.</param>
	public virtual bool HandleGossip(PlayerEntity yapper)
	{
		// Can the player gossip with this humanoid?
		if (!CanGossip(yapper))
			return false;

		// Look up or create the gossip conversation state for this player.
		if (!_gossips.TryGetValue(yapper, out var gossip))
			_gossips.Add(yapper, gossip = new GossipConversation(Server.Now));

		// Reset the gossip state and allow the gossip interaction to proceed.
		gossip.NextGossipTime = Server.Now + GossipCooldown;
			
		return true;
	}
}

public class GossipInteraction : InteractionEntry
{
	private Action<Humanoid, PlayerEntity> _action;
	
	public GossipInteraction(string subject, Action<Humanoid, PlayerEntity> action) : base(new LocalizationEntry(6500013, subject), range: 2)
	{
		_action = action;
	}
	
	public override void OnClick(PlayerEntity yapper, WorldEntity target)
	{
		if (yapper is null || target is not Humanoid humanoid)
			return;

		// Can the player gossip with this humanoid?
		if (humanoid.HandleGossip(yapper) && _action != null)
			_action(humanoid, yapper);
	}
}