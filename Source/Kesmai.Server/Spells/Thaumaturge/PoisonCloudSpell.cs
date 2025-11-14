using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class PoisonCloudSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(39, "Poison Cloud", typeof(PoisonCloudSpell), 24);

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
		return segmentTile.AllowsSpellPath(_caster, this);
	}
		
	public void CastAt(Point2D target, Direction castDirection)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		var segment = _caster.Segment;
		var casterLocation = _caster.Location;
			
		/* Spell can not be cast on the caster's location. */
		var validLocation = (_caster is not PlayerEntity || target != casterLocation);
			
		if (CheckSequence() && validLocation)
		{
			var tile = segment.FindTile(target);

			if (tile != null && AllowCastAt(tile))
			{
				base.OnCast();
					
				var spellPower = _skillLevel;
				var distance = (3 * spellPower) - 27;
		
				var directionFromCaster = Direction.GetDirection(casterLocation, target);

				tile.Add(PoisonCloud.Construct(Color.White, this, (int)spellPower, (int)distance, castDirection,
					(directionFromCaster != Direction.East && directionFromCaster != Direction.West)));

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
		var last = directions.LastOrDefault() ?? Direction.None;
			
		foreach (var direction in directions)
		{
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target))
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(target);
				
			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(target) > 3 || !AllowCastAt(segmentTile))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
		}
			
		CastAt(target, last);
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var directions = Direction.Parse(arguments).ToList();

		if (directions.All(d => d != Direction.None))
		{
			CastAt(directions);
			return true;
		}

		return false;
	}
		
	private class InternalTarget : Target
	{
		private PoisonCloudSpell _spell;
			
		public InternalTarget(PoisonCloudSpell spell) : base(10, TargetFlags.Path | TargetFlags.Direction)
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