using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

/*
Poison has a delay period before it is activated.  The ghouls, gargs, lurkers, Orugugras, drakes and serpents 
poison, have a two-round delay while the scorpions and manticoras have a one-round delay.  Once activated, 
the poison will hit for several consecutive rounds, in decreasing measure. The ghoul's poison, for example, 
will hit for 15 points damage two rounds after the bite.  On the third round, it will hit for 14 points, on 
the fourth for 13, on the fifth for 12, and so on until it runs its course.  If one were to be bitten twice 
by the ghoul, the poison could be hitting for above 25 points in every round, coupled with any other damage 
being received.  This can become quite serious very quickly again, so that by the time you come out of the stun, 
you may have 60 hits per round of damage to deal with.

There are several ways one can deal with poison.  One is for a thief or thaum to CAST NEUTRALIZE upon you, 
eliminating the poison.  Another is to buy some "sprigs" (sold in Kesmai outside the Apothecary shop and 
east of the stairs near the temple in Oakvael); sprigs can be taken from the sack and eaten in one round, and 
will cut the poison damage by about 5 points.  A neutralize poison amulet worn around the neck or held in the 
hand can be used by typing in command mode CAST NEUTRALIZE. 

A very effective way sounds a bit strange at first; that is to drink another poison, with a very long activation 
delay. Wine (bought at the tavern) is a mild poison (does 1 hit of damage) that has a 10 round delay.  Since no 
poison is injected into the system until the delay period of the last received poison is finished, a poisoned 
player can drink some wine and get a 10 round respite from poison effects; enough time to locate a thief or 
thaum to come to the player's position with a NEUTRALIZE spell, find a neutralize amulet or to get a pile of 
balms together in anticipation of the coming damage.
 */
public partial class PoisonStatus : SpellStatus
{
	public override bool Hidden => true;

	private MobileEntity _source;
	private List<Poison> _poisons;

	private Timer _internalTimer;
	
	public List<Poison> Poisons => _poisons;
	
	public int Potency => _poisons.Sum(p => p.Potency);
	
	public PoisonStatus(MobileEntity entity, MobileEntity source, Poison poison) : base(entity)
	{
		_source = source;
		_poisons = new List<Poison>();
			
		Add(poison);

		_internalTimer = entity.Facet.Schedule(poison.Delay, TimeSpan.FromSeconds(1 * 3.0), OnTick);
	}
	
	public void Add(Poison poison)
	{
		if (poison != null)
			_poisons.Add(poison.Clone());
	}

	public void Remove(Poison poison)
	{
		_poisons.Remove(poison);

		if (!_poisons.Any())
			_entity.RemoveStatus(this);
	}
		
	public override void OnRemoved()
	{
		if (_internalTimer != null && _internalTimer.Running)
			_internalTimer.Stop();
			
		base.OnRemoved();
	}
		
	private void OnTick()
	{
		if (_entity is null)
			return;

		var potency = Potency;
			
		if (!_entity.Deleted && _entity.IsAlive && potency > 0)
		{
			/* Calculate damage. */
			var damage = potency;

			if (_entity.HasStatus(typeof(PoisonProtectionStatus)))
				damage /= 2;

			if (_entity is PlayerEntity)
				_entity.SendLocalizedMessage(6300302);

			damage = Math.Max(damage, 1);
				
			/* Determine if direct damage. */
			var direct = false;
			var directPotency = _poisons.Where(p => p.IsDirect).Sum(p => p.Potency);

			if (Utility.RandomBetween(1, potency) <= directPotency)
				direct = true;
				
			/* Apply damage */
			_entity.RegisterIncomingDamage(_source, damage, direct);

			if (_source is not null)
				_source.RegisterOutgoingDamage(_entity, damage, direct);
				
			_entity.ApplyDamage(_source, damage);

			/* Reduce the potency of each stack. */
			_poisons.ForEach(p => p.Potency--);

			/* If after reduction the new potency is 0, we remove the status effect. */
			if (Potency <= 0)
				_entity.RemoveStatus(this);
		}
		else
		{
			_entity.RemoveStatus(this);
		}
	}
}
	
public class Poison
{
	public TimeSpan Delay { get; set; }

	private int _potency;
		
	public int Potency
	{
		get => _potency;
		set => _potency = Math.Max(0, value);
	}

	public bool IsDirect { get; set; }

	public Poison(TimeSpan delay, int potency, bool isDirect = false)
	{
		Delay = delay;
		Potency = potency;

		IsDirect = isDirect;
	}

	public virtual Poison Clone()
	{
		return new Poison(Delay, Potency);
	}
}