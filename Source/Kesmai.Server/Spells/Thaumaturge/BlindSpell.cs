using System;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to cause temporary blindness in an individual.  First warm the spell, then double 
left click on the spell icon in the warmed-spell rack.  The mouse cursor will change to a crosshair; place the 
crosshair over the target creature and left click once to cast the spell.  

Most creatures will just wander about aimlessly if they are blinded, and will be unable to attack. 
*/
public class BlindSpell : DelayedSpell
{
	public static bool IsBlinded(MobileEntity target, double skillLevel, out int rounds)
	{
		var blinded = true;
		
		/* Resistance */
		if (!target.HasStatus(typeof(BlindFearProtectionStatus)))
		{
			var resistance = target.Stats[EntityStat.BlindResistance].Value;
			var resistanceSave = resistance - skillLevel;

			if (Utility.Random(1, 20) <= resistanceSave)
				blinded = false;
		}
		else
		{
			blinded = false;
		}

		rounds = 0;

		if (blinded)
			rounds = (int)Math.Max(skillLevel - target.Level, 0);

		return blinded;
	}
	
	private static SpellInfo _info = new SpellInfo(2, "Blind", typeof(BlindSpell), 4);

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
		    (target is not CreatureEntity creature || !creature.HasImmunity(typeof(BlindSpell))))
		{
			base.OnCast();

			var attacker = _caster;

			if (!target.HasStatus(typeof(BlindStatus)))
			{
				var skillLevel = _skillLevel;

				if (IsBlinded(target, skillLevel, out var rounds))
				{
					foreach (var defender in target.Group.Members)
					{
						attacker.RegisterOutgoingDamage(defender, 0, false);
						defender.RegisterIncomingDamage(attacker, 0, false);
							
						defender.Blind(rounds + 6);
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
		private BlindSpell _spell;

		public InternalTarget(BlindSpell spell) : base(flags: TargetFlags.Harmful)
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