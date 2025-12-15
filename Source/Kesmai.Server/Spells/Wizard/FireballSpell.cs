using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Many Wizards consider this their most important spell.  Fireball, in its simplest form, causes a three-by-three-hex
area (30 feet by 30 feet) to be engulfed in fire.  Any creatures standing in the blast will take damage from the 
flames (assuming they are not fire resistant).  

The spell typically does five times the magic skill level of the Wizard in hit points of damage.  If thrown through
or over an illusion, the fireball is weakened by the magical resistance of the terrain.

To throw a fireball, first warm the spell, then double left click on the spell icon in the warmed-spell rack; the 
mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement 
path.  Cast the spell by double left clicking on the final (target) hex.  

The path must lead to a place you can see, passing only through places you can see.  If you are not equipped with 
some form of fire protection, pay careful attention to where the three-by-three area will fall.

When the Wizard becomes a Shaper of Fire (Skill Level 6), the radius of the fireball can be varied.  If you wish 
to adjust the radius, you must do so before you warm the spell.  Open the spell palette and left click once on the
Fireball spell icon; use the intensity bar on the spell palette to adjust the power of the spell.  After adjustment,
warm and cast the spell as described above.  

The intensity of the flames will increase as the radius is decreased, and vice versa.
*/
public class FireballSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(19, "Fireball", typeof(FireballSpell), 5)
	{
		Intensities = (entity) =>
		{
			var minimum = (entity.GetSkillLevel(Skill.Magic) >= 6 ? 1 : default(int?));
			var maximum = (entity.GetSkillLevel(Skill.Magic) >= 6 ? 3 : default(int?));
				
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
		return segmentTile.AllowsSpellPath(_caster, this) && !segmentTile.HasFlags(ServerTileFlags.Water);
	}
		
	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var mapTile = segment.FindTile(target);

			/* The spell fails if the target hex contains water. */
			if (mapTile != null && AllowCastAt(mapTile))
			{
				base.OnCast();
					
				segment.PlaySound(target, 69, 3, 6);

				var facet = _caster.Facet;
				var damage = 5 * _skillLevel;
				var duration = facet.TimeSpan.FromRounds(1);
					
				var intensity = 2;

				if (_intensity > 0)
					intensity = _intensity;

				var radius = intensity - 1;

				damage /= (intensity * 0.4);

				var visibility = MapVisibility.Calculate(segment, target, radius, false);

				for (var dx = -radius; dx <= radius; dx++)
				{
					for (var dy = -radius; dy <= radius; dy++)
					{
						var fireball = segment.FindTile(target, dx, dy);
						var checkLOS = (mapTile != fireball);

						if (fireball != null && AllowCastAt(fireball))
						{
							if (!checkLOS || visibility.InLOS(fireball.Location))
								fireball.Add(Fire.Construct(Color.White, this, (int)damage, duration, true));
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
		private FireballSpell _spell;
			
		public InternalTarget(FireballSpell spell) : base(10, TargetFlags.Path)
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