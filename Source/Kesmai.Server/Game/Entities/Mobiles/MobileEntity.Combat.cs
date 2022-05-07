using System;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game
{
	public abstract partial class MobileEntity
	{
		/// <summary>
		/// Stuns the entity for a specified number of rounds.
		/// </summary>
		[WorldForge]
		public void Stun(int ticks)
		{
			var roundTimer = _roundTimer;
			var delay = TimeSpan.Zero;

			if (roundTimer != null && roundTimer.Running)
			{
				delay = roundTimer.Next - Server.Now;

				_roundTimer.Stop();
				_roundTimer = null;
			}
			else if (GetStatus<StunStatus>() is StunStatus stunStatus)
			{
				if (stunStatus.Ticks <= ticks)
					return;

				RemoveStatus(stunStatus);
			}
			else if (GetStatus<FearStatus>() is FearStatus fearStatus)
			{
				RemoveStatus(fearStatus);
			}

			QueueRoundTimer(Facet.TimeSpan.FromRounds(ticks / 3.0) + delay);

			AddStatus(new StunStatus(this, ticks));
		}
	}
}