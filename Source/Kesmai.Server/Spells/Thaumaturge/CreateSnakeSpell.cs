using System;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class CreateSnakeSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(17, "Create Snake", typeof(CreateSnakeSpell), 7);

	public override string Name => _info.Name;
	
	protected override bool CheckSequence()
	{
		var segment = _caster.Segment;

		if (segment.GetSubregion(_caster.Location) is TownSubregion)
			return false;
			
		return base.CheckSequence();
	}
		
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
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (_caster != target && CheckSequence())
		{
			var skill = _skillLevel;
			var maxSnakes = skill / 4;

			var valid = (_caster is not PlayerEntity p) || p.Followers.Count < maxSnakes;

			if (valid)
			{
				base.OnCast();
					
				var snake = new SummonedSnake()
				{
					Combatant = target,
				};

				snake.Alignment = _caster.Alignment;
					
				if (_caster is PlayerEntity pCaster)
					snake.Director = pCaster;

				CreatureGroup.Instantiate(snake, target.Facet, target.Segment, target.Location);
					
				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
			else
			{
				Fizzle();
			}

			_caster.EmitSound(29, 3, 6);
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
		private CreateSnakeSpell _spell;

		public InternalTarget(CreateSnakeSpell spell) : base(flags: TargetFlags.Harmful)
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