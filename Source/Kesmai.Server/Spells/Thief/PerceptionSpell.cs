using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class PerceptionSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(95, "Perception", typeof(PerceptionSpell), 30);

	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			base.OnCast();

			var facet = _caster.Facet;
			var rounds = (8 * _skillLevel);
			var duration = TimeSpan.FromSeconds(rounds * 3.0);

			if (!_caster.GetStatus(typeof(PerceptionStatus), out var status))
			{
				status = new PerceptionStatus(_caster)
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