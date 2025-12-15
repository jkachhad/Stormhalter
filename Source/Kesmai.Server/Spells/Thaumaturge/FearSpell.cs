using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to create fear in the hearts of the enemy, and cause them to run away.  Of course, there 
are creatures that know no fear, and thus are unaffected by this spell; fortunately, they are the exception rather 
than the rule.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack.  The mouse cursor will change 
to a crosshair; place the crosshair over the target creature or creatures, and left click once to cast the spell.
*/
public class FearSpell : DelayedSpell
{
	public static bool IsFeared(MobileEntity target, double skillLevel, out int rounds)
	{
		var feared = true;
		
		/* Resistance */
		if (!target.HasStatus(typeof(BlindFearProtectionStatus)))
		{
			var resistance = target.Stats[EntityStat.FearResistance].Value;
			var resistanceSave = resistance - skillLevel;

			if (Utility.Random(1, 20) <= resistanceSave)
				feared = false;
		}
		else
		{
			feared = false;
		}

		rounds = 0;

		if (feared)
			rounds = (int)Math.Max(skillLevel - target.Level, 0);

		return feared;
	}
	
	private static SpellInfo _info = new SpellInfo(83, "Fear", typeof(FearSpell), 4);
		
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
			
		if (_caster != target && CheckSequence() &&
		    (target is not CreatureEntity creature || !creature.HasImmunity(typeof(FearSpell))))
		{
			base.OnCast();
				
			var attacker = _caster;

			attacker.EmitSound(230, 3, 6);

			if (!target.HasStatus(typeof(FearStatus)))
			{
				var skillLevel = _skillLevel;

				if (IsFeared(target, skillLevel, out var rounds))
				{
					foreach (var defender in target.Group.Members)
					{
						attacker.RegisterOutgoingDamage(defender, 0, false);
						defender.RegisterIncomingDamage(attacker, 0, false);
							
						defender.Fear(rounds + 10);
					}
				}

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
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
		private FearSpell _spell;

		public InternalTarget(FearSpell spell) : base(flags: TargetFlags.Harmful)
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
