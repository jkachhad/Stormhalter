using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("StaircaseComponent")]
public class Staircase : ActiveTeleporter
{
	private bool _descends;
		
	public bool Descends => (_elevationDelta < 0) || _descends;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Staircase"/> class.
	/// </summary>
	public Staircase(XElement element) : base(element)
	{
		if (_teleporter != null && Miscellaneous.Terrain.GetAction(_teleporter.ID, out var actionType))
			_descends = (actionType == ActionType.Down);
	}

	/// <summary>
	/// Checks the action to perform a teleport.
	/// </summary>
	protected override bool CheckTeleport(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		var descends = Descends;

		if ((descends && action == ActionType.Down) || (!descends && action == ActionType.Up))
			return true;

		return false;
	}

	/// <summary>
	/// Called before the specified entity is teleported.
	/// </summary>
	protected override void OnBeforeTeleport(WorldEntity entity)
	{
		if (entity is MobileEntity mobile)
		{
			var soundId = (!Descends ? 57 : 58);

			mobile.EmitSound(soundId, 3, 6);

			if (_destinationTile != null)
				_destinationTile.PlaySound(soundId, 3, 6);
		}
	}
		
	protected override bool CanTeleport(SegmentTile parent, ItemEntity entity)
	{
		if (Descends)
			return (Utility.Random(1, 10) <= 1);

		return false;
	}
}