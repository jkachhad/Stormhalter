using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thieves and Wizards can gain the ability to create a short-lived opening through any normally impassable 
material, with only a few exceptions.  First, warm the spell, then double left click on the spell icon in 
the warmed-spell rack; the mouse cursor changes to a crosshair.  Portals can only be created in hexes 
adjacent to the spell caster. Click on an adjacent hex for the spell to target. Cast the spell by double
left clicking on the target hex – i.e., the wall in which you wish to create an opening.

The spell creates a hole in a wall, which lasts for a number of rounds proportional to the magic skill level 
of the spell caster.  Take care not to be standing in the portal when it closes!
*/
public class CreatePortalSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(40, "Create Portal", typeof(CreatePortalSpell), 5);
		
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
		
	public void CastAt(Point2D location)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		var segment = _caster.Segment;
		var casterLocation = _caster.Location;
			
		/* Spell can not be cast on the caster's location. */
		if (CheckSequence() && location != casterLocation)
		{
			if (location != casterLocation && _caster.GetDistanceToMax(location) <= 1)
			{
				var mapTile = segment.FindTile(location);

				if (mapTile != null)
				{
					var walls = mapTile.GetComponents<Wall>()
						.Where(wall => !wall.IsIndestructible).ToList();

					if (walls.Any())
					{
						base.OnCast();
							
						var replaced = new List<TerrainComponent>(walls);

						foreach (var replace in replaced)
							mapTile.Remove(replace);

						var facet = _caster.Facet;
						var duration = facet.TimeSpan.FromRounds(_skillLevel);

						mapTile.Add(Portal.Construct(Color.White, duration, replaced));

						segment.PlaySound(location, 233, 3, 6);

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
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var directions = Direction.Parse(arguments).ToList();

		if (directions.Count <= 1 && directions.All(d => d != Direction.None))
		{
			var location = source.Location;
			var direction = directions.FirstOrDefault() ?? Direction.None;
				
			CastAt(location + direction);
			return true;
		}

		return false;
	}
		
	private class InternalTarget : Target
	{
		private CreatePortalSpell _spell;
			
		public InternalTarget(CreatePortalSpell spell) : base(1, TargetFlags.Path | TargetFlags.Direction)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			var location = source.Location;
			var direction = Direction.None;

			if (path.Count > 0)
				direction = path.FirstOrDefault();
				
			_spell.CastAt(location + direction);
		}
	}
}
