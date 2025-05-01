using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

/*
Thieves and Wizards use this spell to create regions of darkness.  First place the Darkness spell icon in the 
warmed-spell rack.  When the spell is warmed, double left click on the spell icon in the warmed-spell rack; the 
mouse cursor changes to a crosshair.  Click out the path for the spell to follow as you would click out a movement 
path.  Cast the spell by double left clicking on the target hex, which will serve as the center of the shroud of 
darkness.

Wizards who reach Skill Level 5 (Apprentice to Illusions) can control the power of the spell.  If you wish to 
adjust the spell's power, you must do so before you warm the spell.  Open the spell palette and left click once 
on the Darkness spell icon; use the intensity bar on the spell palette to adjust the power of the spell.  After 
adjustment, warm and cast the spell the same as described above.

Both the radius and the duration of the region of darkness are varied by the power of the spell (the intensity of 
the darkness is constant).  The maximum radius and duration of the darkness will increase with your magic skill 
level.  

Thieves cannot vary the power of their Darkness spell.
 */
public class DarknessSpell : DelayedSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(12, "Darkness", typeof(DarknessSpell), 4)
	{
		Intensities = (entity) =>
		{
			var minimum = default(int?);
			var maximum = default(int?);
				
			if (entity is PlayerEntity player)
			{
				if (player.Profession == Profession.Wizard)
				{
					minimum = (entity.GetSkillLevel(Skill.Magic) >= 5 ? 1 : default(int?));
					maximum = (entity.GetSkillLevel(Skill.Magic) >= 5 ? 2 : default(int?));
				}
				else if (player.Profession == Profession.Thief)
				{
					minimum = (entity.GetSkillLevel(Skill.Magic) >= 13 ? 1 : default(int?));
					maximum = (entity.GetSkillLevel(Skill.Magic) >= 13 ? 2 : default(int?));
				}
			}
				
			return (minimum, 2, maximum);
		},
	};
	
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
			var facet = _caster.Facet;
			var segment = _caster.Segment;
			var mapTile = segment.FindTile(target);

			if (mapTile != null && AllowCastAt(mapTile))
			{
				base.OnCast();
					
				var skill = _skillLevel;
				var duration = 3.0 * (10 + skill);

				var intensity = 1;

				if (_intensity > 0)
					intensity = _intensity;

				if (intensity is 1)
					duration *= 1.25;
				
				var dispelDelay = TimeSpan.FromSeconds(duration);

				var directions = (intensity > 1) ? Direction.All : new[] { Direction.None };

				foreach (var direction in directions)
				{
					var segmentTile = segment.FindTile(target + direction);

					if (segmentTile != null)
					{
						if (segmentTile.ContainsComponent<Fire>() || !AllowCastAt(segmentTile))
							continue;

						if (segmentTile.GetComponent<Darkness>() is Darkness darkness)
							segmentTile.Remove(darkness);
							
						segmentTile.Add(Darkness.Construct(Color.White, dispelDelay));
					}
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
		CastAt(_caster.Location + directions);
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
		private DarknessSpell _spell;
			
		public InternalTarget(DarknessSpell spell) : base(7, TargetFlags.Path)
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