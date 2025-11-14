using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class SpeedSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(73, "Speed", typeof(SpeedSpell), 20);

	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			base.OnCast();

			var facet = _caster.Facet;
			var rounds = (8 * _skillLevel - 44);
			var duration = facet.TimeSpan.FromRounds(rounds);

			if (!_caster.GetStatus(typeof(SpeedStatus), out var status))
			{
				status = new SpeedStatus(_caster)
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId },
				};
				status.AddSource(new SpellSource(_caster, duration));

				_caster.AddStatus(status);

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
			else
			{
				status.AddSource(new SpellSource(_caster, duration));
			}

			_caster.EmitSound(232, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}