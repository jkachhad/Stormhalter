using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class LightSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(28, "Light", typeof(LightSpell), 3)
	{
		Intensities = (entity) => (1, 1, 2),
	};
	
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
		
	public bool AllowCastAt(SegmentTile segmentTile)
	{
		return segmentTile.AllowsSpellPath(_caster, this);
	}
		
	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			void castLight(SegmentTile segmentTile, bool dispelDarkness, bool dispelWeb)
			{
				if (dispelDarkness)
				{
					var darknessComponents = segmentTile.GetComponents<Darkness>().ToList();

					foreach (var darkness in darknessComponents)
						darkness.Dispel(segmentTile);
				}

				if (dispelWeb)
				{
					var webComponents = segmentTile.GetComponents<Web>().ToList();

					foreach(var web in webComponents)
						web.Burn(segmentTile);
				}
			}

			var segment = _caster.Segment;
			var targetTile = segment.FindTile(target);

			if (targetTile != null && AllowCastAt(targetTile))
			{
				base.OnCast();
					
				var intensity = 1;

				if (_intensity > 0)
					intensity = _intensity;

				if (intensity > 1)
				{
					foreach (var direction in Direction.All)
					{
						var castTile = segment.FindTile(target + direction);

						if (castTile != null && AllowCastAt(castTile))
							castLight(castTile, true, true);
					}
				}
				else
				{
					castLight(targetTile, true, true);
				}
					
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
		
	public void CastAt(List<Direction> directions)
	{
		var segment = _caster.Segment;
		var target = _caster.Location;

		foreach (var direction in directions)
		{
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target))
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(next);
				
			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(next) > 3 || !AllowCastAt(segmentTile))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
		}
			
		CastAt(target);
	}

	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		if (!String.IsNullOrEmpty(arguments))
		{
			var directions = Direction.Parse(arguments).ToList();

			if (directions.All(d => d != Direction.None))
				CastAt(directions);
		}
		else
		{
			CastAt(_caster.Location);
		}

		return true;
	}
		
	private class InternalTarget : Target
	{
		private LightSpell _spell;
			
		public InternalTarget(LightSpell spell) : base(7, TargetFlags.Path)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(path);
		}
	}
}