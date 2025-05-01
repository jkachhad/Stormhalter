using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public abstract class DragonBreathSpell : DelayedSpell, IWorldSpell
{
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

	public virtual bool AllowCastAt(SegmentTile segmentTile)
	{
		return segmentTile.AllowsSpellPath(_caster, this);
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
			var location = _caster.Location;

			var sourceTile = _caster.SegmentTile;

			/* Spell can not be cast on the caster's location. */
			var validLocation = (_caster is not PlayerEntity || startTile != sourceTile);
				
			if (startTile != null && validLocation)
			{
				base.OnCast();
					
				OnPlace(location);
					
				var spellPower = _skillLevel;

				if (!AllowCastAt(startTile))
				{
					startTile = sourceTile;
					direction = direction.Opposite;
				}

				var visibility = MapVisibility.Calculate(segment, startTile.Location, 3, false);
				var startLocation = startTile.Location;

				void placeBreath(SegmentTile tile)
				{
					if (tile != null && visibility.InLOS(tile.Location) && AllowCastAt(tile))
						PlaceBreath(tile, (int)spellPower);
				}

				for (var i = 0; i <= 2; i++)
				{
					if (!direction.IsDiagonal)
					{
						for (var j = -i; j <= i; j++)
						{
							if (direction != Direction.North && direction != Direction.South)
								placeBreath(segment.FindTile(startLocation, direction.Dx * i, j));
							else
								placeBreath(segment.FindTile(startLocation, j, direction.Dy * i));
						}
					}
					else
					{
						for (var j = 0; j != (direction.Dx * (3 - i)); j += direction.Dx)
							placeBreath(segment.FindTile(startLocation, j, direction.Dy * i));
					}
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

	protected virtual void PlaceBreath(SegmentTile tile, int spellPower)
	{
	}

	protected virtual void OnPlace(Point2D location)
	{
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
		
	public class InternalTarget : Target
	{
		private DragonBreathSpell _spell;
			
		public InternalTarget(DragonBreathSpell spell) : base(1, TargetFlags.Path | TargetFlags.Direction)
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
	
public class DragonBreathLightningSpell : DragonBreathSpell
{
	private static SpellInfo _info = new SpellInfo(66, "Dragon Breath: Lightning", typeof(DragonBreathLightningSpell), 18);

	public override string Name => _info.Name;
	
	protected override void PlaceBreath(SegmentTile tile, int spellPower)
	{
		tile.Add(Lightning.Construct(Color.White, this, spellPower, 12 * spellPower));
	}
		
	protected override void OnPlace(Point2D location)
	{
		_caster.Segment.PlaySound(location, 66, 3, 6);
	}
}

public class DragonBreathFireSpell : DragonBreathSpell
{
	private static SpellInfo _info = new SpellInfo(67, "Dragon Breath: Fire", typeof(DragonBreathFireSpell), 18);

	public override string Name => _info.Name;
	
	public override bool AllowCastAt(SegmentTile segmentTile)
	{
		return base.AllowCastAt(segmentTile) && !segmentTile.HasFlags(ServerTileFlags.Water);
	}

	protected override void PlaceBreath(SegmentTile tile, int spellPower)
	{
		tile.Add(Fire.Construct(Color.White, this, 10 * spellPower, TimeSpan.FromSeconds(3.0), true));
	}
		
	protected override void OnPlace(Point2D location)
	{
		_caster.Segment.PlaySound(location, 69, 3, 6);
	}
}
	
public class DragonBreathIceSpell : DragonBreathSpell
{
	private static SpellInfo _info = new SpellInfo(68, "Dragon Breath: Ice", typeof(DragonBreathIceSpell), 18);

	public override string Name => _info.Name;
	
	protected override void PlaceBreath(SegmentTile tile, int spellPower)
	{
		tile.Add(IceStorm.Construct(Color.White, this, 15 * spellPower, TimeSpan.FromSeconds(3.0)));
	}
		
	protected override void OnPlace(Point2D location)
	{
		_caster.Segment.PlaySound(location, 70, 3, 6);
	}
}