using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges and Thieves use this spell to neutralize the effects of poison.  The spell will completely neutralize 
all poison in the recipient's system when it is cast.  

To cast this spell on yourself, first warm the spell, then double left click on the spell icon in the warmed-spell 
rack.  The mouse cursor will change to a crosshair; place the crosshair over your character's icon, and left click 
once to cast the spell. 

To cast the spell on another person, place the crosshair over the target character's icon, and left click once to 
cast the spell.  Note that there is always a chance that the Thaumaturge's Neutralize spell may fail; a Thief's 
Neutralize spell will not fail, except in the case of insufficient magic points.  This spell can also be cast by 
non-magic users wearing a certain magic amulet.
*/
public class NeutralizePoisonSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(35, "Neutralize Poison", typeof(NeutralizePoisonSpell), 4);

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
				
			if (target.IsPoisoned)
			{
				target.NeutralizePoison(_caster);

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}

			target.EmitSound(232, 3, 6);
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
		private NeutralizePoisonSpell _spell;
			
		public InternalTarget(NeutralizePoisonSpell spell) : base(flags: TargetFlags.Beneficial)
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
