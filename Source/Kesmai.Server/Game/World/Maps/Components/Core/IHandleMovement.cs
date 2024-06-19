﻿namespace Kesmai.Server.Game;

public interface IHandleMovement
{
	/// <summary>
	/// Called when a mobile entity enters this component.
	/// </summary>
	void OnEnter(MobileEntity entity, bool isTeleport);

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	void OnLeave(MobileEntity entity, bool isTeleport);
		
	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	int GetMovementCost(MobileEntity entity);
}