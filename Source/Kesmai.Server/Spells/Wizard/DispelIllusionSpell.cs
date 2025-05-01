using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class DispelIllusionSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(9, "Dispel Illusion", typeof(DispelIllusionSpell), 7);
		
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
		
	public void CastAt(Direction direction)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var target = _caster.Location + direction;

			var segmentTile = segment.FindTile(target);

			if (segmentTile != null)
			{
				var currentIllusion = segmentTile.GetComponent<Illusion>();

				if (currentIllusion != null)
				{
					base.OnCast();
						
					// TODO: Maybe prevent an illusion from dispel if the source vs caster skill is too low.
					currentIllusion.Dispel(segmentTile);

					_caster.EmitSound(232, 3, 6);

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
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		if (!String.IsNullOrEmpty(arguments))
		{
			var directions = Direction.Parse(arguments).ToList();

			if (directions.Count <= 1 && directions.All(d => d != Direction.None))
				CastAt(directions.FirstOrDefault() ?? Direction.None);
		}
		else
		{
			CastAt(Direction.None);
		}

		return true;
	}

	private class InternalTarget : Target
	{
		private DispelIllusionSpell _spell;

		public InternalTarget(DispelIllusionSpell spell) : base(1, TargetFlags.Path)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
                					
			if (path.Count > 0)
				_spell.CastAt(path.FirstOrDefault());
			else
				_spell.CastAt(Direction.None);
		}
	}
}