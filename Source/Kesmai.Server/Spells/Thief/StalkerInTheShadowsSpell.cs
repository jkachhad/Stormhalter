using System;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StalkerInTheShadowsSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(94, "Stalker In The Shadows", typeof(StalkerInTheShadowsSpell), 26);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			if (_caster.HasStatus(typeof(HideStatus)))
			{
				base.OnCast();
	
				if (_caster.GetStatus(typeof(StalkerInTheShadowsStatus), out var status))
					_caster.RemoveStatus(status);
					
				status = new StalkerInTheShadowsStatus(_caster)
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId, }
				};
				status.AddSource(new SpellSource(_caster, TimeSpan.Zero));

				_caster.AddStatus(status);

				_caster.EmitSound(232, 3, 6);

				if (_caster is PlayerEntity player && _item == null)
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