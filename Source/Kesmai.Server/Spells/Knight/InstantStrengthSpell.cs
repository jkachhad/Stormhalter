using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges and Knights cast this spell, which imbues the recipient with temporary strength.  It is standard 
practice for Knights and Thaumaturges to keep the Strength spell active at all times.  The spell icon will appear 
in the active-spell rack of the recipient for as long as the spell is in effect.  The period of protection 
increases with magic skill level.

Magic rings which provide strength spell protection are sometimes found; the spell does not have to be cast 
when the ring is worn.

Thaumaturges
To cast this spell on yourself, first warm the spell, then double left click on the spell icon in the 
warmed-spell rack.  The mouse cursor will change to a crosshair; place the crosshair over your character's 
icon, and left click once to cast the spell.  To cast the spell on another person, place the crosshair over 
the target character's icon, and left click once to cast the spell. 

Knights
To cast this spell on yourself, double left click on the spell icon.  The mouse cursor will change to a 
crosshair; place the crosshair over your character's icon, and left click once to cast the spell.  To cast 
the spell on another person, place the crosshair over the target character's icon, and left click once to 
cast the spell.   
*/
public class InstantStrengthSpell : InstantSpell
{
	private static SpellInfo _info = new SpellInfo(102, "Strength", typeof(InstantStrengthSpell), 3);

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

		if (CheckSequence())
		{
			base.OnCast();

			var facet = _caster.Facet;
			var rounds = (3 * 10 + 35);
			var duration = TimeSpan.FromSeconds(rounds * 3.0);

			if (!target.GetStatus(typeof(StrengthSpellStatus), out var status))
			{
				status = new StrengthSpellStatus(target)
				{
					Inscription = new SpellInscription() { SpellId = _info.SpellId, }
				};
				status.AddSource(new SpellSource(_caster, duration));

				target.AddStatus(status);
			}
			else
			{
				status.AddSource(new SpellSource(_caster, duration));
			}

			target.EmitSound(233, 3, 6);

			if (_caster != target || _item != null)
				target.SendLocalizedMessage(6300311, 553);
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
				entity = source.FindMobileByName(name);
			else
				entity = source;

			if (entity == default(MobileEntity))
				return false;

			Warm(source);
			CastAt(entity);
			return true;
		}

		return base.OnCastCommand(source, arguments);
	}

	private class InternalTarget : MobileTarget
	{
		private InstantStrengthSpell _spell;

		public InternalTarget(InstantStrengthSpell spell) : base(flags: TargetFlags.Beneficial)
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