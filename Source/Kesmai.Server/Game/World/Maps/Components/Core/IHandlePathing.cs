using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public interface IHandlePathing
{
	/// <summary>
	/// Assigns a priority to this pathing handler. Larger values are higher priority.
	/// </summary>
	int PathingPriority { get; }
	
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args);

	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity));

	/// <summary>
	/// Determines whether the specified entity can path the specified spell over this component.
	/// </summary>
	bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell));
}