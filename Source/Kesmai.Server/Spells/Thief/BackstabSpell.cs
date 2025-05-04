using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Pathing;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

// TODO: Damage
// TODO: Crit chance
// TODO: Cooldown.
public class BackstabSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(96, "Backstab", typeof(BackstabSpell), 12);

	public override string Name => _info.Name;
	
	public override bool AllowInterrupt => false;
		
	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}
		
	public void CastAt(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;

		var clearRound = false;
			
		if (_caster != target && CheckSequence())
		{
			var distance = _caster.GetDistanceToMax(target.Location);
			var isRangeValid = (distance > 1);
				
			if (!isRangeValid && distance > 0)
			{
				var (_,darkness) = _caster.GetComponentInLocation<Darkness>();

				if (darkness != default(Darkness) && !target.HasStatus(typeof(NightVisionStatus)))
					isRangeValid = true;
			}
				
			var path = new MovementPath(_caster, target.Location, _caster.Movement, FastAStarAlgorithm.Movement);
			var isPathValid = path.Success;
				
			if (isRangeValid && isPathValid)
			{
				base.OnCast();

				if (_caster.RequestPath(path.Directions.ToArray()))
				{
					if (_caster.RightHand is Dagger rightDagger)
						_caster.Attack(target, rightDagger, out var swingDelay);

					if (_caster.LeftHand is Dagger leftDagger)
						_caster.Attack(target, leftDagger, out var swingDelay);

					clearRound = true;

					if (_caster is PlayerEntity player && _item == null)
						player.AwardMagicSkill(this);
				}
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

		if (clearRound)
			_caster.ClearRound();
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var name = match.Groups[1].Value;
			var entity = source.FindMobileByName(name);

			if (entity == default(MobileEntity))
				return false;

			Warm(source);
			CastAt(entity);
			return true;
		}

		return base.OnCastCommand(source, arguments);
	}

	protected override bool CheckSequence()
	{
		if (!base.CheckSequence())
			return false;

		if (!_caster.HasStatus(typeof(HideStatus)) || !_caster.HasStatus(typeof(SpeedStatus)))
			return false;
			
		return (_caster.RightHand is Dagger || _caster.LeftHand is Dagger);
	}

	private class InternalTarget : MobileTarget
	{
		private BackstabSpell _spell;

		public InternalTarget(BackstabSpell spell) : base(flags: TargetFlags.Harmful)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(target);
		}

		protected override void OnTargetCancel(MobileEntity source, TargetCancel cancelType)
		{
			base.OnTargetCancel(source, cancelType);

			if (source.Spell != _spell)
				return;
				
			_spell.Cancel();
		}
	}
}