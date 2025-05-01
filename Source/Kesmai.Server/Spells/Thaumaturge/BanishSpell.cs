using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Game.Demons;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges use this spell to banish a demon or phantasm, returning it to whence it came (see Summon Demon/Summon
Phantasm).  First warm the spell, then double left click on the spell icon in the warmed-spell rack.  The mouse
cursor will change to a crosshair; place the crosshair over the target creature, and left click once to cast
the spell.  If you mistakenly attempt to apply the spell to a non-demon or non-phantasm, the spell will fail
and magic points will be lost.
    
If you are targeting a demon that you have summoned previously, casting this spell will return the demon to the
eternal flames.  If you attempt to banish a demon summoned by someone else, the demon may or may not be banished,
depending upon the relative strength of your willpower against the demon's willpower.
*/
public class BanishSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(1, "Banish", typeof(BanishSpell), 7);

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
			var attacker = _caster;
			var defender = target;
				
			if (defender is IPhantasm || defender is IDemon)
			{
				base.OnCast();
					
				defender.Kill();

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
			else
			{
				Fizzle();
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
		private BanishSpell _spell;

		public InternalTarget(BanishSpell spell) : base(flags: TargetFlags.Harmful)
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