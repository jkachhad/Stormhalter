using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thaumaturges and Wizards both use this spell, which is similar to Firestorm in that it also creates an immobile 
locus of energy.  Whereas the energy of a firestorm is fire, the energy of a lightning storm is electricity.  

For the time that the lightning storm exists, lightning bolts will strike the spot in which it resides; in addition,
the spell will throw off a number of lightning bolts in random directions, up to two spaces away.  Both the 
duration of the lightning storm and the number of lightning bolts it produces are directly related to the spell 
caster’s magic skill level.  

To create a lightning storm, first warm the spell, then double left click on the spell icon in the warmed-spell 
rack; the mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out
a movement path.  Cast the spell by double left clicking on the final (target) hex.  The path must lead to a 
place you can see, passing through only places you can see.
*/
public class LightningStormSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(31, "Lightning Storm",typeof(LightningStormSpell), 32);

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
					
				segment.PlaySound(target, 66, 3, 6);
					
				var spellPower = _skillLevel;
				var damage = 5 * spellPower;

				tile.Add(LightningStorm.Construct(Color.White, this, (int)damage, (int)spellPower));

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
			var segmentTile = segment.FindTile(target);
				
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
		}
			
		CastAt(target);
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
		private LightningStormSpell _spell;
        	
		public InternalTarget(LightningStormSpell spell) : base(10, TargetFlags.Path)
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