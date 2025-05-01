using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Engines.Pathing;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards casting this spell create a stream of fire, originating from the fingertips, that travels along a specific 
path. The stream of fire affects every creature in its path unless those creatures are immune to fire.  

To throw a fire bolt, first warm the spell, then double left click on the spell icon in the warmed-spell rack; the 
mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement
path.  Cast the spell by double left clicking on the final (target) hex.  The path must lead to a place you can 
see, passing through only places you can see.  

A fire bolt does not have to follow a straight line; it may bend at a 90-degree angle for each space it moves 
through, but it can never turn back toward you.  If the fire bolt strikes solid terrain -- a wall, for example -- 
or if the path is invalid, the fire bolt will terminate before it travels the full distance of the specified path.
The higher the Wizard's magic skill, the longer the length of the fire bolt that can be created. 

The following table shows Wizard skill titles and associated fire bolt lengths:

Skill Level/Title				Length (in contiguous hexes)

12 - Master of Illusions	   	6
13 - Master of Air	   			7
14 - Mage			   			7
15 - Lord of Fire		   		8
16 - Lord of Illusions		   	8
17 - Lord of Air		   		9
18 - Archmage		   			9
19 - Magus	   	          		10

A fire bolt typically does damage equal to eight times the magic skill level of the Wizard.  

When the Wizard becomes a Lord of Fire (Skill Level 15), the spell may be directed at a creature in lieu of 
specifying a path.  After warming the spell, double left click on the spell icon.  The mouse cursor will change 
to a crosshair; place the crosshair over the targeted character's icon and left click once to cast the spell.
*/
public class FireBoltSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(20, "Firebolt", typeof(FireBoltSpell), 10);

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
		var spellPower = _skillLevel;
		var maxLength = (int)Math.Ceiling(spellPower / 2);
			
		var flags = TargetFlags.Path | TargetFlags.Direction;

		if (spellPower >= 15)
			flags |= (TargetFlags.Mobiles | TargetFlags.Harmful);
			
		_caster.Target = new InternalTarget(this, maxLength, flags);
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
		
	public void CastAt(List<SegmentTile> path)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			if (path.Any())
			{
				base.OnCast();

				var facet = _caster.Facet;
				var spellPower = _skillLevel;
				var damage = 8 * spellPower;
				var duration = TimeSpan.FromSeconds(1 * 3.0);

				var maxLength = (int)Math.Ceiling(spellPower / 2);
				var adjustedPath = path.Take(maxLength).ToList();

				for (var i = 0; i < adjustedPath.Count(); i++)
				{
					var tile = adjustedPath[i];

					if (!AllowCastAt(tile))
						break;
						
					tile.Add(Fire.Construct(Color.White, this, (int)damage, duration, true));
				}

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);

				_caster.EmitSound(224, 3, 6);
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
		var spellPath = new List<SegmentTile>();
				
		var segment = _caster.Segment;
		var start = _caster.Location;
				
		var target = start;
		var lastDirection = Direction.None;

		foreach (var direction in directions)
		{
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target) || direction == lastDirection.Opposite)
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(next);

			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through or contains water,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(target) > 3 || !AllowCastAt(segmentTile))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
			lastDirection = direction;
				
			spellPath.Add(segmentTile);
		}
			
		CastAt(spellPath);
	}

	public void CastAt(MobileEntity mobile)
	{
		var spellPower = _skillLevel;
		var maxLength = (int)Math.Ceiling(spellPower / 2);
			
		var pathing = new MovementPath(_caster, mobile.Location, maxLength, new SpellAlgorithm(this));

		if (pathing.Success)
			CastAt(pathing.Directions);
		else
			OnCast();
	}

	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var spellPower = _skillLevel;

		if (spellPower >= 15)
		{
			var match = Regex.Match(arguments, @"^at (.*)$", RegexOptions.IgnoreCase);

			if (match.Success)
			{
				var name = match.Groups[1].Value;
				var entity = _caster.FindMobileByName(name);

				if (entity == default(MobileEntity))
					return false;

				CastAt(entity);
				return true;
			}
		}

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
		private FireBoltSpell _spell;
			
		public InternalTarget(FireBoltSpell spell, int range, TargetFlags flags) : base(range, flags)
		{
			_spell = spell;
		}
			
		protected override void OnTarget(MobileEntity source, object target)
		{
			if (source.Spell != _spell)
				return;
				
			var caster = _spell.Caster;
				
			if (target is MobileEntity mobile && mobile.IsAlive)
			{
				if (caster.InRange(mobile) && caster.InLOS(mobile) && caster.CanSee(mobile))
					_spell.CastAt(mobile);
			}
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(path);
		}
	}
}