using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Wizards use this spell to create a flowing mass of molten rock three spaces wide and of variable length.  The 
total length of the lava flow is dependent upon the Wizard’s magic skill level.  The temperature of the lava is 
not nearly as hot as that produced by an actual volcano, so it tends to be already somewhat congealed.  The lava 
flow advances one to two spaces at a time, and it can knock people down.  

First warm the spell, then double left click on the spell icon in the warmed-spell rack.  The mouse cursor changes
to a crosshair.  Beginning from one of the eight spaces adjacent to the Wizard, click out the path for the spell
to follow as you would click out a movement path.  Cast the spell by double left clicking on the final (target) 
hex.  The lava will flow away from your character in the chosen direction.

Note:  The initial space where the spell is cast cannot be solid, i.e., a wall, mountain, secret door, etc.  If 
the initial space is not empty, the spell will begin in the space the Wizard is standing in, with . . . 
unpredictable results.
*/
public class CreateLavaSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(27, "Create Lava", typeof(CreateLavaSpell), 33);
		
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
		
	public void CastAt(Direction direction)
	{
		CastAt(direction, Direction.None);
	}

	public void CastAt(Direction direction, Direction offset)
	{
		var segment = _caster.Segment;
		var location = _caster.Location;

		var startTile = segment.FindTile(location + direction + offset);

		if (startTile != null)
			CastAt(startTile, direction);
	}

	public void CastAt(SegmentTile startTile, Direction direction)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
	
		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var sourceTile = _caster.SegmentTile;
				
			segment.PlaySound(sourceTile.Location, 351, 6, 12);

			/* Spell can not be cast on the caster's location. */
			var validLocation = (_caster is not PlayerEntity || startTile != sourceTile);
				
			if (startTile != null && validLocation)
			{
				base.OnCast();
					
				var spellPower = _skillLevel;

				if (!startTile.AllowsSpellPath())
					startTile = sourceTile;
					
				startTile.Add(Lava.Construct(Color.White, this, (int)spellPower, 14, direction));
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
			CastAt(directions.FirstOrDefault() ?? Direction.Cardinal.Random());
			return true;
		}

		return false;
	}
		
	private class InternalTarget : Target
	{
		private CreateLavaSpell _spell;
			
		public InternalTarget(CreateLavaSpell spell) : base(1, TargetFlags.Path | TargetFlags.Direction)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			if (path.Count > 0)
				_spell.CastAt(path.FirstOrDefault());
			else
				_spell.CastAt(Direction.Cardinal.Random());
		}
	}
}