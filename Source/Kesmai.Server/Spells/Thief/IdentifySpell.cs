using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class IdentifySpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(32, "Identify", typeof(IdentifySpell), 3);
		
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
		
	void CastAt(ItemEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			if (target != null)
			{
				base.OnCast();
					
				target.Identified = true;

				_caster.EmitSound(237, 3, 6);
				_caster.SendDescription(target);

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

	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (left|right)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var hand = match.Groups[1].Value;

			if (hand.Matches("left", true))
				CastAt(_caster.LeftHand);
			else
				CastAt(_caster.RightHand);
		}
		else
		{
			Cast();
		}

		return true;
	}

	private sealed class InternalTarget : ItemTarget
	{
		private readonly IdentifySpell _spell;

		public InternalTarget(IdentifySpell spell)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, ItemEntity target)
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
