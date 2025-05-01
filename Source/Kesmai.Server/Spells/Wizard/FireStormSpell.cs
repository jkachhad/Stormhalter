using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create an immobile locus of fire in the space specified by the Wizard.  Each round that 
the firestorm exists, a fireball will strike the spot in which it is cast; in addition, the firestorm will throw 
off a number of fireballs in random directions, up to three spaces away.  Both the duration of the firestorm and 
the number of fireballs it produces are directly related to the Wizard’s magic skill level.  

To create a firestorm, first warm the spell, then double left click on the spell icon in the warmed-spell rack; 
the mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a 
movement path.  Cast the spell by double left clicking on the final (target) hex.  The path must lead to a place 
you can see, passing through only places you can see. 
*/
public class FireStormSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(21, "Firestorm", typeof(FireStormSpell), 16);
		
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
			_caster.EmitSound(69, 6, 12);
				
			var segment = _caster.Segment;

			var spellPower = _skillLevel;
			var damage = 10 * spellPower;

			var mapTile = segment.FindTile(target);

			/* The spell fails if the target hex contains water. */
			if (mapTile != null && AllowCastAt(mapTile))
			{
				base.OnCast();
					
				mapTile.Add(FireStorm.Construct(Color.White, this, (int)damage, (int)spellPower));

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
		private FireStormSpell _spell;
        	
		public InternalTarget(FireStormSpell spell) : base(10, TargetFlags.Path)
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