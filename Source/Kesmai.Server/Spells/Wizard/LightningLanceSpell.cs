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
Wizards cast this spell to create a tongue of electricity, which leaps from the Wizard’s finger at a creature, or 
alternately traces a path specified by the Wizard in the same manner as Fire Bolt.  This spell differs from the 
Thaumaturge's Lightning Bolt spell in that it extends horizontally from the Wizard’s hand, as opposed to the 
lightning bolt's vertical strike.  

To throw a lightning lance, first warm the spell, then double left click on the spell icon in the warmed-spell 
rack; the mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click 
out a movement path.  Cast the spell by double left clicking on the final (target) hex.  The path must lead to 
a place you can see, passing through only places you can see.  

Like a fire bolt, a lightning lance does not have to follow a straight line; it may bend at a 90-degree angle 
for each space that it moves through, but it can never turn back toward you.  Care must be taken to ensure that
the initial space the lightning lance is created in is not some solid terrain, such as a mountain or a wall.

Alternatively, the Wizard may cast this spell at a creature as long as the creature is within the Wizard’s sight.
After warming the spell, double left click on the spell icon.  The mouse cursor will change to a crosshair; 
place the crosshair over the targeted character's icon.  Left click once to cast the spell. Creatures in 
intervening spaces  also will be electrocuted.

It should be noted that casting a lightning lance at a drake will only irritate it, since drakes are immune to 
electricity.  This spell typically does about 12 times the Wizard’s magic skill level in hit points of damage.  
Beware of casting a lightning lance at a creature charging into the same space you are standing in; the results 
can be shocking.
 */
public class LightningLanceSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(30, "Lightning Lance", typeof(LightningLanceSpell), 20);

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

	public void CastAt(List<SegmentTile> path)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		/* Spell can not be cast on the caster's location. */
		var validLocation = (_caster is not PlayerEntity || !path.Contains(_caster.SegmentTile));
			
		if (CheckSequence() && validLocation)
		{
			_caster.EmitSound(66, 3, 6);
				
			if (path.Any())
			{
				base.OnCast();
					
				var spellPower = _skillLevel;
				var damage = 12 * spellPower;

				var maxLength = (int)Math.Ceiling(spellPower / 2);
				var adjustedPath = path.Take(maxLength).ToList();

				for (var i = 0; i < adjustedPath.Count(); i++)
				{
					var tile = adjustedPath[i];
						
					if (!AllowCastAt(tile))
						break;

					tile.Add(Lightning.Construct(Color.White, this, (int)spellPower, (int)damage));
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
		var spellPath = new List<SegmentTile>();
				
		var segment = _caster.Segment;
		var start = _caster.Location;
		var currentTile = segment.FindTile(start);
			
		var target = start;
		var lastDirection = Direction.None;

		foreach (var direction in directions)
		{
			/* If the current tile contains water, we interrupt the path. */
			if (currentTile.HasFlags(ServerTileFlags.Water))
				break;
				
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target) || direction == lastDirection.Opposite)
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(next);

			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through or contains water,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(target) > 3 || !AllowCastAt(segmentTile) 
			                                         || segmentTile.HasFlags(ServerTileFlags.Water))
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
		var pathing = new MovementPath(_caster, mobile.Location, 4, new SpellAlgorithm(this));

		if (pathing.Success)
			CastAt(pathing.Directions);
		else
			OnCast();
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
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
		private LightningLanceSpell _spell;
			
		public InternalTarget(LightningLanceSpell spell) : base(7, TargetFlags.Mobiles | TargetFlags.Harmful | TargetFlags.Path | TargetFlags.Direction)
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
				
			// TODO: Goes through doors??
			_spell.CastAt(path);
		}
	}
}