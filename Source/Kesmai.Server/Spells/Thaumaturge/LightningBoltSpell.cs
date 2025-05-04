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
Thaumaturges use this spell to summon a bolt of lightning, which emanates from the air above the target, and 
strikes everything within the 10-by-10 region of effect.  A lightning bolt typically will do six times the magic 
skill level of the Thaumaturge in hit points of damage.  

The effect of a lightning bolt is not influenced by the nature of the terrain over which it is thrown, with the 
exception of water.  Lightning is thrown by the Thaumaturge at a target space by defining a path to it. 

To throw a lightning bolt, first warm the spell, then double left click on the spell icon in the warmed-spell rack;
the mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a 
movement path.  Cast the spell by double left clicking on the final (target) hex.  The path must lead to a place 
you can see, passing only through places you can see.  A word of warning:  if you path this spell over water, the 
lightning bolt will not cross the water and will instead strike the last hex before the water.

Thaumaturges may have to lead a target to hit it with a lightning bolt, which sometimes makes the spell difficult 
to use. When a Thaumaturge gains the title of Seer at the ninth skill level, the spell may be directed at a 
creature in lieu of specifying a path.  After warming the spell, double left click on the spell icon.  The mouse 
cursor will change to a crosshair; place the crosshair over the targeted character's icon.  Left click once to cast
the spell.	 
 */
public class LightningBoltSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(29, "Lightning", typeof(LightningBoltSpell), 5);

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
		var flags = TargetFlags.Path;

		if (spellPower >= 9)
			flags |= (TargetFlags.Mobiles | TargetFlags.Harmful);
			
		_caster.Target = new InternalTarget(this, flags);
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
					
				segment.PlaySound(target, 66, 3, 6);
					
				var spellPower = _skillLevel;
				var damage = 6 * spellPower;

				tile.Add(Lightning.Construct(Color.White, this, (int)spellPower, (int)damage));

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
		var currentTile = segment.FindTile(target);
				
		foreach (var direction in directions)
		{
			/* If the current tile contains water, we interrupt the path. */
			if (currentTile.HasFlags(ServerTileFlags.Water))
				break;
				
			/* We continue adding a direction until our target is out of LOS. */
			if (!_caster.InLOS(target))
				break;
				
			var next = target + direction;
			var segmentTile = segment.FindTile(next);
				
			/* We've extended beyond the max visibility, fizzle the spell. 
			 * If the next tile does not allow pathing through or contains water,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(next) > 3 || !AllowCastAt(segmentTile) 
			                                       || segmentTile.HasFlags(ServerTileFlags.Water))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
		}
			
		CastAt(target);
	}
		
	public void CastAt(MobileEntity mobile)
	{
		CastAt(mobile.Location);
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		var spellPower = _skillLevel;

		if (spellPower >= 9)
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
		private LightningBoltSpell _spell;

		public InternalTarget(LightningBoltSpell spell, TargetFlags flags) : base(10, flags)
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
					_spell.CastAt(mobile.Location);
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