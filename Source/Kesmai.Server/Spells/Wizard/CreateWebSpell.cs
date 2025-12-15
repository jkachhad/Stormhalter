using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create a web similar to a web made by a spider.  This web prevents creatures from 
passing through it, and may immobilize creatures standing in it.  As with Bonfire, you may direct the spell 
to any place that you can see.  The web covers a ten by ten foot area.  The web has a long duration, but it 
may be also be burned or dispelled.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack; the mouse cursor 
changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement path.  
Cast the spell by double left clicking on the final (target) hex, where you wish to create a web.

The path must lead to a place you can see, passing only through places you can see.
*/
public class CreateWebSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(56, "Create Web", typeof(CreateWebSpell), 4);

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
			var segmentTile = segment.FindTile(target);

			if (segmentTile != null && AllowCastAt(segmentTile))
			{
				base.OnCast();
					
				segment.PlaySound(target, 231, 3, 6);

				var facet = _caster.Facet;
				var rounds = (_skillLevel + 6);
				var duration = facet.TimeSpan.FromRounds(rounds);
						
				if (!segmentTile.ContainsComponent<Fire>())
					segmentTile.Add(Web.Construct(Color.White, duration, true));

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
		private CreateWebSpell _spell;
			
		public InternalTarget(CreateWebSpell spell) : base(10, TargetFlags.Path)
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
