using System;
using System.Linq;
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
		
		/// <summary>
		/// Gets a value indicating if this entity can be poisoned by a specific <see cref="Poison"/>.
		/// </summary>
		[WorldForge]
		public virtual bool AllowPoison(Poison poison)
		{
			return true;
		}

		/// <summary>
		/// Poisons the entity using the specified <see cref="Poison"/>.
		/// </summary>
		[WorldForge]
		public void Poison(MobileEntity source, Poison poison)
		{
			if (!AllowPoison(poison))
				return;
			
			if (GetStatus<PoisonStatus>() is PoisonStatus status)
			{
				if (poison is Venom)
				{
					var venomStack = status.Poisons.OfType<Venom>().ToList();

					if (venomStack.Count >= 4)
					{
						foreach (var venom in venomStack.Take(venomStack.Count - 3))
							status.Remove(venom);
					}
				}

				status.Add(poison);
			}
			else
			{
				AddStatus(new PoisonStatus(this, source, poison));
			}
		}

		/// <summary>
		/// Neutralizes all <see cref="Poison">Poisons</see>.
		/// </summary>
		[WorldForge]
		public void NeutralizePoison(MobileEntity source = default(MobileEntity))
		{
			if (GetStatus(typeof(PoisonStatus), out var status))
			{
				RemoveStatus(status);

				if (source != null)
					SendLocalizedMessage(6300304, source.Name);
				else
					SendLocalizedMessage(6300303);
			}
		}

		/// <summary>
		/// Clears <see cref="PoisonStatus"/>.
		/// </summary>
		[WorldForge]
		public void ClearPoison()
		{
			if (GetStatus(typeof(PoisonStatus), out var status))
				RemoveStatus(status);
		}
	}
}