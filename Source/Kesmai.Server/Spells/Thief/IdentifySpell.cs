using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Commands;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class IdentifySpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(32, "Identify", typeof(IdentifySpell), 3);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		CastAt(_caster.RightHand);
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

				_caster.EmitSound(226, 3, 6);
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
			CastAt(_caster.RightHand);
		}

		return true;
	}
}