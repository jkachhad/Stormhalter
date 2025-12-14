using System;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ForcefieldStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;

	private int _minimumAbsorb;
	private int _maximumAbsorb;
		
	public int Power { get; private set; }

	public ForcefieldStatus(MobileEntity entity, int power, int minAbsorb, int maxAbsorb) : base(entity)
	{
		Power = power;
			
		_minimumAbsorb = minAbsorb;
		_maximumAbsorb = maxAbsorb;
	}

	public bool Absorb(int incomingDamage, out int absorbed)
	{
		var minimum = _minimumAbsorb;
		var maximum = Math.Min(_maximumAbsorb, Power);

		var chance = (80 - _maximumAbsorb);

		if (Utility.RandomBetween(1, 100) < chance)
			absorbed = Utility.RandomBetween(minimum, maximum);
		else
			absorbed = 0;

		Power -= absorbed;

		if (Power <= 0)
			_entity.RemoveStatus(this);
			
		return (absorbed > 0);
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 611); /* The spell of [Forcefield] has worn off. */
		}
	}
}