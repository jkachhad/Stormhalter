using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class ProtectionFromBlindFearSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(41, "Protection From Blind and Fear", typeof(ProtectionFromBlindFearSpell), 27);

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
		
	public virtual void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var facet = _caster.Facet;
			var rounds = (3 * _skillLevel + 35) * ProtectionSpellDurationMultiplier;
			var duration = facet.TimeSpan.FromRounds(rounds);

			if (!target.GetStatus(typeof(BlindFearProtectionStatus), out var status))
			{
				status = new BlindFearProtectionStatus(target)
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

			target.EmitSound(233, 3, 6);

			if (_caster != target || _item != null)
				target.SendLocalizedMessage(6300311, 541);
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

			CastAt(entity);
		}
		else
		{
			CastAt(_caster);
		}

		return true;
	}
		
	private class InternalTarget : MobileTarget
	{
		private ProtectionFromBlindFearSpell _spell;

		public InternalTarget(ProtectionFromBlindFearSpell spell) : base(flags: TargetFlags.Beneficial)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(target);
		}
	}
}
