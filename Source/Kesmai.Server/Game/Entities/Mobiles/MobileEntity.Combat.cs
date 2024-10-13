using System;
using System.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

public abstract partial class MobileEntity
{
	/// <summary>
	/// Calculates the damage modifier against the specified item.
	/// </summary>
	public int CalculateDamageModifier(ItemEntity item)
	{
		var damageModifier = 0;

		/* Calculate armor bonus from chest pieces. */
		var paperdoll = Paperdoll;
			
		if (paperdoll is null)
			return 0;
			
		var armor = paperdoll.Armor;
			
		if (armor != null)
			damageModifier += armor.GetArmorBonus(item);

		return damageModifier;
	}
		
	/// <summary>
	/// Checks if the <see cref="ShieldPenetration"/> value can penetrate the shield for this instance.
	/// </summary>
	/// <returns>Returns true if the attack is not blocked by a shield buff.</returns>
	[WorldForge]
	public virtual bool CheckShieldPenetration(ShieldPenetration penetration)
	{
		if (GetStatus<ShieldStatus>() is ShieldStatus status)
		{
			var currentProtection = Stats[EntityStat.Barrier].Value;

			if (penetration > ShieldPenetration.None)
			{
				var penetrationDice = 0;
	
				switch (penetration)
				{
					case ShieldPenetration.VeryHeavy: penetrationDice = 90; break; // Very Heavy
					case ShieldPenetration.Heavy: penetrationDice = 45; break; // Heavy
					case ShieldPenetration.Medium: penetrationDice = 36; break; // Medium
					case ShieldPenetration.Light: penetrationDice = 18; break; // Light
					case ShieldPenetration.VeryLight: penetrationDice = 13; break; // Very Light
				}
	
				return Utility.RandomBetween(1, penetrationDice) > currentProtection;
			}

			return false;
		}

		return true;
	}
	
	/// <summary>
	/// Dazes the entity for a specified number of rounds.
	/// </summary>
	[WorldForge]
	public void Daze(int ticks)
	{
		if (IsInvulnerable)
			return;
			
		/* Remove any fear effect. */
		if (GetStatus<FearStatus>() is FearStatus fearStatus)
			RemoveStatus(fearStatus);
			
		/* Remove any stun effect. */
		if (GetStatus<StunStatus>() is StunStatus stunStatus)
			RemoveStatus(stunStatus);
			
		var delay = TimeSpan.Zero;
			
		/* Remove any existing daze effect. */
		if (GetStatus<DazeStatus>() is DazeStatus currentDaze)
		{
			if (ticks <= currentDaze.Ticks)
				return;

			ticks = currentDaze.Ticks + 1;

			RemoveStatus(currentDaze);
		}
		else
		{
			/* Take any existing time in their round, and add it to the daze. */
			if (_roundTimer != null && _roundTimer.Running)
			{
				delay = _roundTimer.Next - Server.Now;

				_roundTimer.Stop();
				_roundTimer = null;
			}
		}
			
		/* Start the daze. */
		AddStatus(new DazeStatus(this, ticks, delay));
	}
		
	/// <summary>
	/// Stuns the entity for a specified number of rounds.
	/// </summary>
	[WorldForge]
	public void Stun(int ticks)
	{
		/* Remove any fear effect. */
		if (GetStatus<FearStatus>() is FearStatus fearStatus)
			RemoveStatus(fearStatus);
			
		/* Remove any stun effect. */
		if (GetStatus<DazeStatus>() is DazeStatus dazeStatus)
			RemoveStatus(dazeStatus);
			
		var delay = TimeSpan.Zero;
			
		/* Remove any existing stun effect. */
		if (GetStatus<StunStatus>() is StunStatus currentStun)
		{
			if (ticks <= currentStun.Ticks)
				return;

			ticks = currentStun.Ticks + 1;

			RemoveStatus(currentStun);
		}
		else
		{
			/* Take any existing time in their round, and add it to the stun. */
			if (_roundTimer != null && _roundTimer.Running)
			{
				delay = _roundTimer.Next - Server.Now;

				_roundTimer.Stop();
				_roundTimer = null;
			}
		}
			
		/* Start the stun. */
		AddStatus(new StunStatus(this, ticks, delay));
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