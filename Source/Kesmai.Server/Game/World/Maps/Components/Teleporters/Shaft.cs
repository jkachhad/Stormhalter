using System;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ShaftComponent")]
public class Shaft : ActiveTeleporter
{
	private int _slipChance;
		
	/// <summary>
	/// Gets or sets the chance to slip while interacting with this shaft.
	/// </summary>
	public int SlipChance
	{
		get => _slipChance;
		set => _slipChance = value;
	}
		
	/* 73 31 1 */
	/// <summary>
	/// Initializes a new instance of the <see cref="Shaft"/> class.
	/// </summary>
	public Shaft(XElement element) : base(element)
	{
		if (element.TryGetElement("slipChance", out var slipChance))
			_slipChance = (int)slipChance;
	}

	/// <summary>
	/// Checks the action to perform a teleport.
	/// </summary>
	protected override bool CheckTeleport(MobileEntity entity, ActionType action)
	{
		var descending = (action == ActionType.Down) || (action == ActionType.ClimbDown);
			
		if (!descending)
			return false;

		if (entity is PlayerEntity player)
		{
			var height = -(_elevationDelta);

			if (action == ActionType.ClimbDown)
			{
				var slipChance = (_slipChance / 100.0) + (player.Encumbrance * 0.05);

				if (Utility.RandomDouble() < slipChance)
					height = -Utility.RandomBetween(_elevationDelta / 2, _elevationDelta);
				else
					height = 0;
			}

			if (height > 0)
				player.Fall(height, _parent, _destinationTile);
		}

		return true;
	}
}