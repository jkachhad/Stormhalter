using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to cause destruction of the internal organs of a targeted victim.  It is one of the 
most potent spells known.  First, warm the spell, then double left click on the spell icon in the warmed-spell 
rack.  The mouse cursor will change to a crosshair; place the crosshair over the target character's icon, and left 
click once to cast the spell. 

If the spell is cast at a group, it will affect one of the group members chosen at random.  A death spell will 
typically do ten times the magic skill level of the Thaumaturge in hit points of damage.	 
*/
public class DeathSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(8, "Death", typeof(DeathSpell), 6);

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

			attacker.EmitSound(225, 3, 6);

			var skillLevel = _skillLevel;
			var baseDamage = 10 * skillLevel;
			var damage = SpellHelper.AdjustDamage((int)baseDamage, 0.9, 1.1);

			if (target.Stats[EntityStat.DeathProtection].Value is 0)
			{
				var resistance = target.Stats[EntityStat.DeathResistance].Value;
				var resistanceSave = resistance - skillLevel;

				if (Utility.Random(1, 20) <= resistanceSave)
					damage /= 2;
			}
			else
			{
				damage /= 2;
			}

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
			player.SendLocalizedMessage(6300316, 6300343); /* You have been hit by a death spell. */
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
		private DeathSpell _spell;

		public InternalTarget(DeathSpell spell) : base(flags: TargetFlags.Harmful)
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
