using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class ProtectionFromFireIceSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(44, "Protection from Fire and Ice", typeof(ProtectionFromFireIceSpell), 21);

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
		
	public void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var gainSkill = false;

			var facet = _caster.Facet;
			var rounds = (3 * _skillLevel + 35) * ProtectionSpellDurationMultiplier;
			var duration = TimeSpan.FromSeconds(rounds * 3.0);

			if (!target.GetStatus(typeof(FireProtectionStatus), out var fireStatus))
			{
				fireStatus = new FireProtectionStatus(target)
				{
					Inscription = new SpellInscription() { SpellId = 43, }
				};
				fireStatus.AddSource(new SpellSource(_caster, duration));

				target.AddStatus(fireStatus);

				gainSkill = true;
			}
			else
			{
				fireStatus.AddSource(new SpellSource(_caster, duration));
			}

			if (!target.GetStatus(typeof(IceProtectionStatus), out var iceStatus))
			{
				iceStatus = new IceProtectionStatus(target)
				{
					Inscription = new SpellInscription() { SpellId = 42, }
				};
				iceStatus.AddSource(new SpellSource(_caster, duration));

				target.AddStatus(iceStatus);

				gainSkill = true;
			}
			else
			{
				iceStatus.AddSource(new SpellSource(_caster, duration));
			}

			if (gainSkill)
			{
				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}

			target.EmitSound(232, 3, 6);

			if (_caster != target || _item != null)
				target.SendLocalizedMessage(6300311, 544);
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
		private ProtectionFromFireIceSpell _spell;

		public InternalTarget(ProtectionFromFireIceSpell spell) : base(flags: TargetFlags.Beneficial)
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