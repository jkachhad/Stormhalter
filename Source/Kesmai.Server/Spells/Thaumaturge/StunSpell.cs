using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to stun individuals or groups of enemies.  First warm the spell, then double left 
click on the spell icon in the warmed-spell rack.  The mouse cursor will change to a crosshair; place the 
crosshair over the icon of the targeted creature or creatures that you wish to strike.  

Left click once to cast the spell.  If the enemies fail to resist the spell, they will be stunned for a period 
of time governed by the difference between the Thaumaturge’s magic skill level and the victim’s experience level. 
A stun spell cannot be broken or extended. 
*/
public class StunSpell : DelayedSpell
{
	public static bool IsStunned(MobileEntity target, double skillLevel, out int rounds)
	{
		var stunned = true;
		
		/* Resistance */
		if (target.Stats[EntityStat.StunProtection].Value is 0)
		{
			var resistance = target.Stats[EntityStat.StunResistance].Value;
			var resistanceSave = resistance - skillLevel;

			if (Utility.Random(1, 20) <= resistanceSave)
				stunned = false;
		}
		else
		{
			stunned = false;
		}

		rounds = 0;

		if (stunned)
			rounds = (int)Math.Max(skillLevel - target.Level, 0);

		return stunned;
	}
	
	private static SpellInfo _info = new SpellInfo(54, "Stun", typeof(StunSpell), 4);

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

		if (_caster != target && CheckSequence() && 
		    (target is not CreatureEntity creature || !creature.HasImmunity(typeof(StunSpell))))
		{
			base.OnCast();

			var attacker = _caster;

			if (!target.HasStatus(typeof(StunStatus)))
			{
				var skillLevel = _skillLevel;

				if (IsStunned(target, skillLevel, out var rounds))
				{
					/* Creatures are stunned for a maximum of 6 rounds, players for 4. */
					var maxRounds = (target is CreatureEntity) ? 5 : 3;
						
					if (rounds > maxRounds)
						rounds = maxRounds;
						
					/* Stun the whole group. */
					foreach (var defender in target.Group.Members)
					{
						attacker.RegisterOutgoingDamage(defender, 0, false);
						defender.RegisterIncomingDamage(attacker, 0, false);
							
						defender.Stun((rounds * 3) + 3); /* An additional round always applies. */
					}
				}

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}

			_caster.EmitSound(229, 3, 6);
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
		private StunSpell _spell;

		public InternalTarget(StunSpell spell) : base(flags: TargetFlags.Harmful)
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