using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to direct a large, sharp, high speed projectile of ice at a creature.  The velocity of the 
ice shard is such that it is never blocked by armor.  

To throw an ice spear, first warm the spell, then double left click on the spell icon in the warmed-spell rack.  
The mouse cursor will change to a crosshair; place the crosshair over the icon of the poor, unfortunate creature 
that you wish to strike down with the spell.  Left click once to cast the spell.  

This spell will do about twelve times the Wizard’s magic skill level in hit points of damage.  Ice spear will 
affect even those creatures that are normally immune to cold, since not many of them are also immune to being 
struck by a high velocity piece of ice.
*/
public class IceSpearSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(25, "Ice Spear", typeof(IceSpearSpell), 14);

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
		
	public void CastOn(MobileEntity target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction || !target.IsAlive)
			return;
			
		if (_caster != target && CheckSequence())
		{
			base.OnCast();
				
			var attacker = _caster;
			var defender = target;

			attacker.EmitSound(222, 3, 6);

			var skill = _skillLevel;
			var baseDamage = 12.5 * skill;
			var damage = SpellHelper.AdjustDamage((int)baseDamage, 0.9, 1.1);
			
			defender.ScaleSpellDamage(attacker, ref damage, true);
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
			player.SendLocalizedMessage(6300316, 6300319);
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
				CastOn(entity);
				return true;
			}
		}

		return false;
	}
		
	private class InternalTarget : MobileTarget
	{
		private IceSpearSpell _spell;

		public InternalTarget(IceSpearSpell spell) : base(flags: TargetFlags.Harmful)
		{
			_spell = spell;
		}

		protected override void OnTarget(MobileEntity source, MobileEntity target)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastOn(target);
		}
	}
}