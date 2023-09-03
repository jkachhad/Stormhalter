using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public interface IHandlePathing
{
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	void HandleMovementPath(PathingRequestEventArgs args);

	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	bool AllowMovementPath(MobileEntity entity = default(MobileEntity));

	/// <summary>
	/// Determines whether the specified entity can path the specified spell over this component.
	/// </summary>
	bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell));
}