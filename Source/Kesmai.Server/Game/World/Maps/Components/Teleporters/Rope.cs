using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("RopeComponent")]
public class Rope : ActiveTeleporter
{
	private bool _isSecret;
	private int _slipChance;

	/// <summary>
	/// Gets or sets a value indicating whether this instance is secret.
	/// </summary>
	public bool IsSecret
	{
		get => _isSecret;
		set => _isSecret = value;
	}
		
	/// <summary>
	/// Gets or sets the chance to slip while interacting with this rope.
	/// </summary>
	public int SlipChance
	{
		get => _slipChance;
		set => _slipChance = value;
	}

	public bool Descends => (_elevationDelta < 0);
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Rope"/> class.
	/// </summary>
	public Rope(XElement element) : base(element)
	{
		if (element.TryGetElement("isSecret", out var isSecretElement))
			_isSecret = (bool)isSecretElement;
			
		if (element.TryGetElement("slipChance", out var slipChance))
			_slipChance = (int)slipChance;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (!_isSecret)
		{
			foreach (var terrain in base.GetTerrain(parent, beholder))
				yield return terrain;
		}
	}

	/// <summary>
	/// Handles rope interaction. Since rope sprites can appear on angled wall tiles with visual offsets,
	/// we allow interaction when the player is on the rope component's tile or adjacent to it.
	/// This ensures all ropes work properly regardless of visual positioning.
	/// </summary>
	public override bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		var playerLocation = entity.Location;
		var ropeLocation = parent.Location;
		
		// Allow interaction if player is on the rope component's tile
		if (playerLocation == ropeLocation)
			return base.HandleInteraction(parent, entity, action);
		
		// Allow interaction if player is adjacent to the rope component
		var distance = entity.GetDistanceToMax(ropeLocation);
		if (distance <= 1)
			return base.HandleInteraction(parent, entity, action);
		
		return false;
	}

	/// <summary>
	/// Checks the action to perform a teleport.
	/// </summary>
	protected override bool CheckTeleport(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		var descends = (_elevationDelta < 0);
		var climbingDown = (action != ActionType.ClimbUp);

		if ((descends && !climbingDown) || (!descends && climbingDown))
		{
			entity.SendLocalizedMessage(6300233);
			return false;
		}
			
		if (entity is PlayerEntity player)
		{
			if (player.RightHand != null && player.LeftHand != null)
			{
				player.SendLocalizedMessage(6300212);
				return false;
			}
				
			if (!climbingDown && player.Encumbrance >= 4)
			{
				player.SendLocalizedMessage(6300208);
				return false;
			}
				
			var slipChance = (_slipChance / 100.0) + (player.Encumbrance * 0.05);
				
			if (Utility.RandomDouble() < slipChance)
			{
				player.Fall(Math.Abs(Utility.RandomBetween(_elevationDelta / 2, _elevationDelta)), 
					parent, _destinationTile);
					
				return descends;
			}
		}
			
		return true;
	}

	protected override void OnBeforeTeleport(WorldEntity entity)
	{
		if (entity is PlayerEntity player)
			player.ApplyEncumbrance(0);
	}
}