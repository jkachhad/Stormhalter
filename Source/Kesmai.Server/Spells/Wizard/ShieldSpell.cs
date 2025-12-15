using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to provide temporary shielding to its recipient.  To cast this spell on yourself, first 
warm the spell, then double left click on the spell icon in the warmed-spell rack.  The mouse cursor will change
to a crosshair; place the crosshair over your character's icon, and left click once to cast the spell.  To cast
the spell on another person, place the crosshair over the target character's icon, and left click once to cast
the spell.  The period of protection increases with magic skill level.

The spell has a probability of blocking any blow made at the Wizard proportional to the magic skill level of 
the spell caster, and inversely proportional to the mass of the object.  Thus, the spell has greater 
effectiveness against arrows, but is less effective against massive objects like greatswords.  The spell icon
will appear in the active-spell rack of the recipient for as long as the spell is in effect.  

Magic rings which provide shield spell protection are sometimes found; the spell does not have to be cast when
the ring is worn.
*/
public class ShieldSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(52, "Shield", typeof(ShieldSpell), 3);

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
			var rounds = (3 * _skillLevel + 35);
			var duration = facet.TimeSpan.FromRounds(rounds);

			if (!target.GetStatus(typeof(ShieldStatus), out var status))
			{
				status = new ShieldStatus(target)
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
				target.SendLocalizedMessage(6300311, 552);
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
		private ShieldSpell _spell;

		public InternalTarget(ShieldSpell spell) : base(flags: TargetFlags.Beneficial)
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
