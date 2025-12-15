using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create a whirling vortex of air that exists for a number of rounds, dependent upon 
the magic skill level of the Wizard.  Creatures caught in the path of the whirlwind will take some hit points 
from flying debris.  In addition, the whirlwind may knock down smaller creatures.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack; the mouse cursor changes 
to a crosshair.  Click out the path for the spell to follow as you would click out a movement path.  Cast the 
spell by double left clicking on the final (target) hex.  The path must lead to a place you can see, passing 
through only places you can see.  The whirlwind will move in the direction it was cast, throwing off smaller 
whirlwinds in nearby hexes.  A word of warning:  whirlwinds may deflect unpredictably off of solid terrain.
*/
public class WhirlwindSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(57, "Whirldwind", typeof(WhirlwindSpell), 20);
		
	public override string Name => _info.Name;
	
	public int Strength { get; set; }
		
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
				var damage = 12 * spellPower;
				var strength = (Strength > 0 ? Strength : (4 * spellPower) - 40);

				if (strength < 1)
					strength = 1;

				var directionFromCaster = Direction.GetDirection(casterLocation, target);

				tile.Add(Whirlwind.Construct(Color.White, this, (int)damage, (int)strength, castDirection,
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
		var start = _caster.Location;
			
		var target = start;
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
		private WhirlwindSpell _spell;
			
		public InternalTarget(WhirlwindSpell spell) : base(10, TargetFlags.Path | TargetFlags.Direction)
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