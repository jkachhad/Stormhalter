using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to cast a magic missile at a single enemy or a group of enemies.  If cast at a group, 
the missile strikes one of the members of the group at random.  This makes it very useful in fighting an enemy 
that moves rapidly.  

To cast a magic missile, first warm the spell, then double left click on the spell icon in the warmed-spell rack.
The mouse cursor will change to a crosshair; place the crosshair over the icon of the targeted creature or
creatures that you wish to strike.  Left click once to cast the spell.  

Although it is learned early, this spell is also of value to more advanced Wizards because its power increases 
with magic skill level. The spell typically does four times the skill level of the Wizard in hit points of damage.
*/
public class MagicMissileSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(33, "Magic Missle", typeof(MagicMissileSpell), 3);

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
			player.SendLocalizedMessage(6300316, 6300317);
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
		private MagicMissileSpell _spell;

		public InternalTarget(MagicMissileSpell spell) : base(flags: TargetFlags.Harmful)
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