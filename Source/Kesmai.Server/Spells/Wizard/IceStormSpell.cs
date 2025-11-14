using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards may create ice storms, in which raging winds and massive hailstones batter creatures to pulp.  An ice 
storm is cast in exactly the same manner as fireball, and affects the same size area (30 by 30 feet), but is 
about 50 percent more powerful.  A concentrated ice storm is among a Wizard’s more powerful offensive spells.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack; the mouse cursor changes 
to a crosshair.  Click out the path for the spell to follow as you would click out a movement path.  Cast the spell 
by double left clicking on the final (target) hex.  The path must lead to a place you can see, passing through only
 places you can see.

When the Wizard becomes a Shaper of Ice, the radius of the ice storm can be controlled.  If you wish to adjust the 
spell's power, you must do so before you warm the spell.  Open the spell palette and left click once on the Ice 
Storm spell icon; use the intensity bar on the spell palette to adjust the power of the spell.  After adjustment, 
warm and cast the spell as described above.  

The intensity of the storm will increase as the radius is decreased, and vice versa.
*/
public class IceStormSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(26, "Icestorm", typeof(IceStormSpell), 6)
	{
		Intensities = (entity) =>
		{
			var minimum = (entity.GetSkillLevel(Skill.Magic) >= 7 ? 1 : default(int?));
			var maximum = (entity.GetSkillLevel(Skill.Magic) >= 7 ? 3 : default(int?));

			return (minimum, 2, maximum);
		},
	};
	
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

	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var tile = segment.FindTile(target);

			if (tile != null && AllowCastAt(tile))
			{
				base.OnCast();
					
				segment.PlaySound(target, 70, 3, 6);

				var facet = _caster.Facet;
				var spellPower = _skillLevel;
				var damage = 7.5 * spellPower;
				var duration = facet.TimeSpan.FromRounds(1);

				var intensity = 2;

				if (_intensity > 0)
					intensity = _intensity;

				var radius = intensity - 1;

				damage /= (intensity * 0.5);

				var visibility = MapVisibility.Calculate(segment, target, radius, false);

				for (var dx = -radius; dx <= radius; dx++)
				{
					for (var dy = -radius; dy <= radius; dy++)
					{
						var icestorm = segment.FindTile(target, dx, dy);
						var checkLOS = (tile != icestorm);

						if (icestorm != null && AllowCastAt(icestorm))
						{
							if (!checkLOS || visibility.InLOS(icestorm.Location))
								icestorm.Add(IceStorm.Construct(Color.White, this, (int)damage, duration));
						}
					}
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
			var next = target + direction;
				
			if (!_caster.InLOS(next))
				break;

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
		private IceStormSpell _spell;
			
		public InternalTarget(IceStormSpell spell) : base(7, TargetFlags.Path)
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