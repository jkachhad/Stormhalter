using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create a small bonfire, which covers a single space and burns for a period of time 
governed by the Wizard's skill level.  The damage done by the bonfire is comparable to that of a fireball.  
Bonfires are useful for destroying corpses and spider webs, and blocking passageways.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack.  The mouse cursor 
changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement path.  
Cast the spell by double left clicking on the final (target) hex.

The path must lead to a place you can see, passing only through places you can see. 
*/
public class BonfireSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(3, "Bonfire", typeof(BonfireSpell), 3);
		
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
				var rounds = (_skillLevel + 4);
				var duration = TimeSpan.FromSeconds(rounds * 3.0);
					
				mapTile.Add(Fire.Construct(Color.White, this, (int)damage, duration, true));

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
		

	/*
	 * The original description of Legends of Kesmai v1.21b help file suggests that this spell can be only
	 * cast where the caster has visibility. However, in the Lands of Kes implementation, the spell can be
	 * cast out of LOS by 1 hex.
	 */
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
		private BonfireSpell _spell;
			
		public InternalTarget(BonfireSpell spell) : base(10, TargetFlags.Path)
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