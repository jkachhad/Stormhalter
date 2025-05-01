using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

// TODO: Verify the caster has access to the illusions. I think our spell cast check may already check 
// for intensity validation.
public abstract class IllusionSpell : DelayedSpell
{
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
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var target = _caster.Location + direction;

			var tile = segment.FindTile(target);

			if (tile != null && OnCreateIllusion(tile))
			{
				base.OnCast();
					
				_caster.EmitSound(230, 3, 6);

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

	protected virtual bool OnCreateIllusion(SegmentTile tile)
	{
		return true;
	}
		
	public override bool OnCastCommand(MobileEntity source, string arguments)
	{
		if (!String.IsNullOrEmpty(arguments))
		{
			var directions = Direction.Parse(arguments).ToList();

			if (directions.Count <= 1 && directions.All(d => d != Direction.None))
				CastAt(directions.FirstOrDefault() ?? Direction.None);
		}
		else
		{
			CastAt(Direction.None);
		}

		return true;
	}

	public virtual bool AllowMovementPath(MobileEntity entity) => true;

	public virtual void HandleMovementPath(PathingRequestEventArgs args)
	{
	}

	private class InternalTarget : Target
	{
		private IllusionSpell _spell;

		public InternalTarget(IllusionSpell spell) : base(1, TargetFlags.Path)
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
				_spell.CastAt(Direction.None);
		}
	}
}
	
public class IllusionWallSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(59, "Illusion: Wall", typeof(IllusionWallSpell), 9)
	{
		Intensities = (entity) =>
		{
			var minimum = 1;
			var maximum = 1;


			if (entity.GetSkillLevel(Skill.Magic) >= 15)
				maximum = 4;
			else if (entity.GetSkillLevel(Skill.Magic) >= 12)
				maximum = 2;

			return (minimum, minimum, maximum);
		}
	};
	
	public override string Name => _info.Name;
		
	private static List<Terrain> _walls = new List<Terrain>()
	{
		new Terrain(31),  /* 9 */
			
		new Terrain(446), /* 12 */
			
		new Terrain(447), /* 15 */
		new Terrain(448)
	};

	public override bool AllowMovementPath(MobileEntity entity) => false;
		
	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;

		tile.Add(Illusion.Construct(this, _walls[
			(_intensity.Clamp(1, _walls.Count) - 1)
		], PathingResult.Daze));
		return true;
	}
}
	
public class IllusionFireSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(61, "Illusion: Fire", typeof(IllusionFireSpell), 9)
	{
		Intensities = (entity) =>
		{
			var minimum = 1;
			var maximum = 1;

			if (entity.GetSkillLevel(Skill.Magic) >= 14)
				maximum = 2;

			return (minimum, minimum, maximum);
		}
	};

	private static List<Terrain> _fires = new List<Terrain>()
	{
		new Terrain(135), /* 9 */
		new Terrain(135, Color.Orange), /* 14 */
	};
	
	public override string Name => _info.Name;

	public override bool AllowMovementPath(MobileEntity entity)
	{
		return (entity.Stats[EntityStat.FireProtection].Value > 60);
	}
		
	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;
			
		tile.Add(Illusion.Construct(this, _fires[
			(_intensity.Clamp(1, _fires.Count) - 1)
		]));
		return true;
	}
}
	
public class IllusionIceSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(62, "Illusion: Ice",typeof(IllusionIceSpell), 9)
	{
		Intensities = (entity) =>
		{
			var minimum = 1;
			var maximum = 1;

			if (entity.GetSkillLevel(Skill.Magic) >= 15)
				maximum = 3;
			else if (entity.GetSkillLevel(Skill.Magic) >= 12)
				maximum = 2;

			return (minimum, minimum, maximum);
		}
	};
		
	private static List<Terrain> _ice = new List<Terrain>()
	{
		new Terrain(17),				/* 9 */
		new Terrain(134),				/* 12 */
		new Terrain(134, Color.Aqua),	/* 15 */
	};
	
	public override string Name => _info.Name;

	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;
			
		tile.Add(Illusion.Construct(this, _ice[
			(_intensity.Clamp(1, _ice.Count) - 1)
		]));
		return true;
	}
}
	
public class IllusionWaterSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(63, "Illusion: Water", typeof(IllusionWaterSpell), 9)
	{
		Intensities = (entity) =>
		{
			var minimum = 1;
			var maximum = 1;

			if (entity.GetSkillLevel(Skill.Magic) >= 15)
				maximum = 3;
			else if (entity.GetSkillLevel(Skill.Magic) >= 12)
				maximum = 2;

			return (minimum, minimum, maximum);
		}
	};
	
	private static List<Terrain> _waters = new List<Terrain>()
	{
		new Terrain(22),	/* 9 */
		new Terrain(601),	/* 12 */
		new Terrain(374)	/* 15 */
	};
	
	public override string Name => _info.Name;

	public override bool AllowMovementPath(MobileEntity entity)
	{
		if (entity is CreatureEntity creature)
			return creature.CanFly || creature.CanSwim || creature.HasStatus(typeof(BreatheWaterStatus));

		return false;
	}
		
	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;

		tile.Add(Illusion.Construct(this, _waters[
			(_intensity.Clamp(1, _waters.Count) - 1)
		]));
		return true;
	}
}
	
public class IllusionRuinsSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(64, "Illusion: Ruins", typeof(IllusionRuinsSpell), 9)
	{
		Intensities = (entity) =>
		{
			var minimum = 1;
			var maximum = 3;

			if (entity.GetSkillLevel(Skill.Magic) >= 15)
				maximum = 8; 
			else if (entity.GetSkillLevel(Skill.Magic) >= 12)
				maximum = 6;

			return (minimum, minimum, maximum);
		}
	};
	
	private static List<Terrain> _ruins = new List<Terrain>()
	{
		new Terrain(44),	/* 9 */
		new Terrain(45), 
		new Terrain(46), 
			
		new Terrain(170),	/* 12 */
		new Terrain(380), 
		new Terrain(398), 
			
		new Terrain(432),  /* 15 */
		new Terrain(465),
	};

	public override string Name => _info.Name;
	
	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;
			
		tile.Add(Illusion.Construct(this, _ruins[
			(_intensity.Clamp(1, _ruins.Count) - 1)
		], PathingResult.Interrupted)); /* This only applies to creatures. */
		return true;
	}
}
	
// TODO: Update this when we create a horizontal bridge sprite. Maybe when implementing rotations.
public class IllusionBridgeSpell : IllusionSpell
{
	private static SpellInfo _info = new SpellInfo(65, "Illusion: Bridge", typeof(IllusionBridgeSpell), 9);
	private static Terrain _bridge = new Terrain(252);
   
	public override string Name => _info.Name;
	
	protected override bool OnCreateIllusion(SegmentTile tile)
	{
		if (!base.OnCreateIllusion(tile))
			return false;
   
		tile.Add(Illusion.Construct(this, _bridge));
		return true;
	}
}
	
// TODO: New Spell Illusion: Tree