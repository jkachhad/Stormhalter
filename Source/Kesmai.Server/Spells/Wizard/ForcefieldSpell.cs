using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ForcefieldSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(111, "Forcefield", typeof(ForcefieldSpell), 7);

	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var facet = _caster.Facet;
			var rounds = (3 * _skillLevel + 35);
			var power = (int)(20 * _skillLevel);
			var duration = facet.TimeSpan.FromRounds(rounds);

			if (!_caster.GetStatus(typeof(ForcefieldStatus), out var status) && power > 0)
			{
				status = new ForcefieldStatus(_caster, power, 0, (int)(_skillLevel / 2))
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId, }
				};
				status.AddSource(new SpellSource(_caster, duration));

				_caster.AddStatus(status);
				_caster.EmitSound(232, 3, 6);
					
				if (_caster is PlayerEntity player && _item is null)
					player.AwardMagicSkill(this);
			}
			else
			{
				Fizzle();
			}
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}