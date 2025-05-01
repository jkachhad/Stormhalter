using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class BreatheWaterSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(4, "Breathe Water", typeof(BreatheWaterSpell), 6);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}
		
	public override void OnReset()
	{
		if (_caster.Target is InternalTarget)
			Target.Cancel(_caster);
	}

	public void CastOn(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();

			var facet = _caster.Facet;
			var rounds = 100;
			var duration = TimeSpan.FromSeconds(rounds * 3.0);

			if (!target.GetStatus(typeof(BreatheWaterStatus), out var status))
			{
				status = new BreatheWaterStatus(target)
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId, }
				};
				status.AddSource(new SpellSource(_caster, duration));

				target.AddStatus(status);

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
			else
			{
				status.AddSource(new SpellSource(_caster, duration));
			}

			target.EmitSound(232, 3, 6);

			if (_caster != target || _item != null)
				target.SendLocalizedMessage(6300311, 504);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}

	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var name = match.Groups[1].Value;
			var entity = default(MobileEntity);

			if (!name.Matches("self", true))
				entity = _caster.FindMobileByName(name);
			else
				entity = _caster;
				
			if (entity == default(MobileEntity))
				return false;

			CastOn(entity);
		}
		else
		{
			CastOn(_caster);
		}

		return true;
	}

	private class InternalTarget : MobileTarget
	{
		private BreatheWaterSpell _spell;

		public InternalTarget(BreatheWaterSpell spell) : base(flags: TargetFlags.Beneficial)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastOn(target);
		}
	}
}