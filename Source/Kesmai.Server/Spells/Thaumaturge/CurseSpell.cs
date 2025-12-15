using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
The Thaumaturges’ basic attack spell inflicts physical damage upon a creature from a distance.  First, warm the 
spell, then double left click on the spell icon in the warmed-spell rack.  The mouse cursor will change to a 
crosshair; place the crosshair over the target character's icon, and left click once to cast the spell. 

The Curse spell will typically do four times the magic skill level of the Thaumaturge in hit points of damage, 
and is a powerful weapon when used by an advanced Thaumaturge. 
 */
public class CurseSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(7, "Curse", typeof(CurseSpell), 3);

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

		if (_caster != target && CheckSequence())
		{
			base.OnCast();
				
			var attacker = _caster;
			var defender = target;

			attacker.EmitSound(227, 3, 6);

			var skill = _skillLevel;
			var baseDamage = 4 * skill;
			var damage = SpellHelper.AdjustDamage((int)baseDamage, 0.9, 1.1);

			defender.ApplySpellDamage(attacker, this, damage, true);

			if (_caster is PlayerEntity player && _item == null)
				player.AwardMagicSkill(this);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
		
	public override void OnHit(MobileEntity entity)
	{
		if (entity is PlayerEntity player)
			player.SendLocalizedMessage(6300316, 6300342); /* You have been hit by a curse. */
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

		if (match.Success)
		{
			var name = match.Groups[1].Value;
			var entity = _caster.FindMobileByName(name);

			if (entity != default(MobileEntity))
			{
				CastAt(entity);
				return true;
			}
		}

		return false;
	}
		
	private class InternalTarget : MobileTarget
	{
		private CurseSpell _spell;

		public InternalTarget(CurseSpell spell) : base(flags: TargetFlags.Harmful)
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
