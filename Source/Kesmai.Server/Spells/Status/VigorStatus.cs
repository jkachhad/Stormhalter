using System;
using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class VigorStatus : SpellStatus
{
	private Timer _decayTimer;

	public VigorStatus(MobileEntity entity) : base(entity)
	{
	}

	public void Consume()
	{
		var charges = Count;

		charges += _entity.Stamina;

		if (charges > _entity.MaxStamina)
			charges = _entity.MaxStamina;

		_entity.Stamina = 0;
		_entity.ResetRegeneration(false, true, false);

		Count = charges;
	}
			
	public void Activate()
	{
		if (_decayTimer != null)
			_decayTimer.Stop();

		_decayTimer = Timer.DelayCall(TimeSpan.Zero, _entity.Facet.TimeSpan.FromRounds(1), () =>
		{
			if (Count > 0)
				Count--;

			if (Count <= 0)
				_entity.RemoveStatus(this);
		});
	}

	public override void OnRemoved()
	{
		if (_decayTimer != null)
			_decayTimer.Stop();

		_decayTimer = null;
			
		base.OnRemoved();
			
		_entity.PlaySound(221);

		if (_entity.Client != null)
			_entity.SendLocalizedMessage(Color.Magenta, 6300270, 597); /* The spell of [Vigor] has worn off. */
	}
}