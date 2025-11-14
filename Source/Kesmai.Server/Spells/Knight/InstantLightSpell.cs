using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class InstantLightSpell : InstantSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(101, "Light", typeof(InstantLightSpell), 3);
	
	public override string Name => _info.Name;
		
	public override bool AllowInterrupt => false;
		
	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
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
			base.OnCast();
				
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

			if (_item != null)
			{
				if (targetTile != null && AllowCastAt(targetTile))
					castLight(targetTile, true, true);
			}
			else
			{
				foreach (var direction in Direction.All)
				{
					var castTile = segment.FindTile(target + direction);

					if (castTile != null && AllowCastAt(castTile))
						castLight(castTile, true, false);
				}
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
		Warm(source);
			
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
		private InstantLightSpell _spell;
			
		public InternalTarget(InstantLightSpell spell) : base(7, TargetFlags.Path)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			var end = _spell.Caster.Location;

			foreach (var direction in path)
			{
				end += direction;

				if (!source.InLOS(end))
					break;
			}

			_spell.CastAt(end);
		}
			
		protected override void OnTargetCancel(MobileEntity source, TargetCancel cancelType)
		{
			base.OnTargetCancel(source, cancelType);

			if (source.Spell != _spell)
				return;
				
			_spell.Cancel();
		}
	}
}