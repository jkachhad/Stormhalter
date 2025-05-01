using System;
using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create an explosion which can daze and maim creatures caught within the blast radius; it 
can also knock down walls and doors.  The power of the explosion becomes greater as the Wizard’s magic skill 
increases.

The blast radius also increases with the Wizard' s magic skill level.  The full power blast of an eighth level 
wizard may pulverize stone walls inside the blast radius.  Wizards using this spell must be careful, since the 
radius of the blast can be 30 feet or more for advanced Wizards.  Careless use might produce more devastation than 
desired.  

To cast the spell, first warm it then double left click on the spell icon in the warmed-spell rack.  The mouse 
cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement path.  
Cast the spell by double left clicking on the final (target) hex.
*/
public class ConcussionSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(5, "Concussion", typeof(ConcussionSpell), 10);
		
	public override string Name => _info.Name;
	
	public bool Localized { get; set; }
		
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

	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		var facet = _caster.Facet;
		var duration = TimeSpan.FromSeconds(1.0);
		
		var segment = _caster.Segment;
		var casterLocation = _caster.Location;
			
		/* Spell can not be cast on the caster's location. */
		var validLocation = (_caster is not PlayerEntity || target != casterLocation || _item != null);
			
		if (CheckSequence() && validLocation)
		{
			var mapTile = segment.FindTile(target);

			if (mapTile != null && AllowCastAt(mapTile))
			{
				base.OnCast();
					
				var spellPower = _skillLevel;
				var damage = (int)(8 * spellPower);
				
				var intensity = 1;

				if (!Localized)
				{
					if (spellPower >= 12)
						intensity++;
				}
					
				for (var dx = -intensity; dx <= intensity; dx++)
				{
					for (var dy = -intensity; dy <= intensity; dy++)
					{
						var distance = Math.Max(Math.Abs(dx), Math.Abs(dy));
						var segmentTile = segment.FindTile(target, dx, dy);

						if (segmentTile != null)
							segmentTile.Add(Explosion.Construct(this, damage, distance, duration));
					}
				}

				segment.PlaySound(target, 68, 3, 6);

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
			 * If the next tile does not allow pathing through, we interrupt the path. This will cause the
			 * spell the fizzle. */
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
		var directions = Direction.Parse(arguments).ToList();

		if (directions.All(d => d != Direction.None))
		{
			CastAt(directions);
			return true;
		}

		return false;
	}
		
	public override void OnHit(MobileEntity entity)
	{
		if (entity is PlayerEntity player)
			player.SendLocalizedMessage(6300316, 6300378);
	}
		
	private class InternalTarget : Target
	{
		private ConcussionSpell _spell;
			
		public InternalTarget(ConcussionSpell spell) : base(10, TargetFlags.Path | TargetFlags.Direction)
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