using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

/*
Only Thieves have this personal spell, which enables the Thief to hide in the shadows and remain invisible 
to other players.  Players or creatures who are not in the same space, or in an adjacent space, have only a 
small chance of spotting the Thief in any given round.  

The odds of being spotted depend on the Thief’s magic skill level and dexterity, as well as the other player’s 
or creature’s abilities.  If the other player or creature is also a Thief, the odds of hiding successfully are 
reduced considerably.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack.  This is a personal 
spell that can be cast only on the Thief.  The spell icon appears in the active-spell rack for as long as the 
spell is in effect.  The period of protection increases with magic skill level.  

The spell will stay in effect as long as you are wearing no armor heavier than leather, are wielding no large 
weapons, and remain next to a wall or some other solid terrain that casts shadows.  If you are hit by 
anything, or step away from a wall, the spell is broken.  The spell is not broken if you are seen.
 */
public class HideInShadowSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(24, "Hide In Shadows", typeof(HideInShadowSpell), 3);

	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			var leftHand = _caster.LeftHand;
			var rightHand = _caster.RightHand;

			if (_caster.CanHide() && _caster.CanHideWith(leftHand) && _caster.CanHideWith(rightHand))
			{
				base.OnCast(); 
					
				if (!_caster.GetStatus(typeof(HideStatus), out var status))
				{
					status = new HideStatus(_caster)
					{
						Inscription = new SpellInscription() { SpellId = _info.SpellId, }
					};
					status.AddSource(new SpellSource(_caster, TimeSpan.Zero));

					_caster.AddStatus(status);
					_caster.SendLocalizedMessage(Color.Magenta, 6300300); /* You are hiding in the shadows. */

					if (_caster is PlayerEntity player && _item == null)
						player.AwardMagicSkill(this);
				}
				else
				{
					status.AddSource(new SpellSource(_caster, TimeSpan.Zero));
				}
					
				_caster.EmitSound(233, 3, 6);
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
