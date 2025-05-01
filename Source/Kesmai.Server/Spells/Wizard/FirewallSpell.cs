using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class FirewallSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(22, "Wall of Fire", typeof(FirewallSpell), 5);

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
		
	public bool AllowCastAt(SegmentTile segmentTile)
	{
		return segmentTile.AllowsSpellPath(_caster, this) && !segmentTile.HasFlags(ServerTileFlags.Water);
	}
		
	public void CastAt(Point2D target, Direction castOrientation)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		var segment = _caster.Segment;
		var casterLocation = _caster.Location;
			
		/* Spell can not be cast on the caster's location. */
		var validLocation = (_caster is not PlayerEntity || target != casterLocation);
			
		if (CheckSequence() && validLocation)
		{
			var mapTile = segment.FindTile(target);

			if (mapTile != null && AllowCastAt(mapTile))
			{
				base.OnCast();
					
				segment.PlaySound(target, 69, 3, 6);

				var facet = _caster.Facet;
				var spellPower = _skillLevel;
				var damage = 5 * spellPower;
				var duration = TimeSpan.FromSeconds(10 * spellPower);
					
				var directions = new List<Direction>()
				{
					castOrientation, Direction.None, castOrientation.Opposite
				};

				foreach (var direction in directions)
				{
					var wall = segment.FindTile(target + direction);

					/* The spell fails if the target hex contains water. */
					if (wall != null && wall.AllowsSpellPath() && !wall.HasFlags(ServerTileFlags.Water))
						wall.Add(Fire.Construct(Color.White, this, (int)damage, duration, true));
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
		var last = Direction.None;
			
		foreach (var direction in directions)
		{
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target))
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(next);
				
			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through or contains water,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(next) > 3 || !AllowCastAt(segmentTile))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
			last = direction;
		}
			
		CastAt(target, last.Perpendicular);
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
			CastAt(_caster.Location, Direction.All.Random());
		}

		return true;
	}
		
	private class InternalTarget : Target
	{
		private FirewallSpell _spell;
			
		public InternalTarget(FirewallSpell spell) : base(10, TargetFlags.Path | TargetFlags.Direction)
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