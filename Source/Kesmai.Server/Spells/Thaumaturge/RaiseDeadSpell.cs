using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class RaiseDeadSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(46, "Raise Dead", typeof(RaiseDeadSpell), 10);

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

	public void CastAt(Corpse corpse)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			if (corpse != null && corpse.Owner is PlayerEntity dead && !dead.IsAlive)
			{
				base.OnCast();
					
				dead.Teleport(corpse.Location);
				dead.Resurrect(ResurrectType.Spell);
				dead.SendLocalizedMessage(6100035); /* You have been resurrected. */

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
		var handMatch = Regex.Match(arguments, @"^at (left|right)$", RegexOptions.IgnoreCase);

		if (handMatch.Success)
		{
			var hand = handMatch.Groups[1].Value;

			if (hand.Matches("left", true) && _caster.LeftHand is Corpse leftCorpse)
				CastAt(leftCorpse);
			else if (_caster.LeftHand is Corpse rightCorpse)
				CastAt(rightCorpse);

			return true;
		}
			
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var name = match.Groups[1].Value;
			var item = _caster.FindItemsByName(name)
				.OfType<Corpse>()
				.FirstOrDefault();

			if (item == default(Corpse))
				return false;
					
			CastAt(item);
			return true;
		}
			
		return false;
	}
		
	private class InternalTarget : Target
	{
		private RaiseDeadSpell _spell;
			
		public InternalTarget(RaiseDeadSpell spell) : base(7, TargetFlags.Items)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, object target)
		{
			if (source.Spell != _spell)
				return;
				
			if (target is Corpse corpse)
				_spell.CastAt(corpse);
		}
	}
}