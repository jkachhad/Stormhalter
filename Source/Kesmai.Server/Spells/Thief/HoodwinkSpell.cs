using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Items;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class HoodwinkSpell : DelayedSpell, IWorldSpell
{
	public static int Range = 2;
		
	private static SpellInfo _info = new SpellInfo(92, "Hoodwink", typeof(HoodwinkSpell), 8)
	{
		Intensities = (entity) =>
		{
			/* 1 to 10, +1 every two levels. */
			var minimum = 1;
			var maximum = (int)Math.Floor((1 + entity.GetSkillLevel(Skill.Magic)) / 2);

			return (minimum, 1, maximum);
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
		if (_skillLevel >= 7)
		{
			_caster.Target = new InternalTarget(this);
			return;
		}

		CastAt(_caster.Location);
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
			var mapTile = segment.FindTile(target);

			if (mapTile != null && AllowCastAt(mapTile))
			{
				var amount = Utility.RandomRange(_intensity * 10, 0.7, 1.1);
				var cost = amount * 10;

				if (_caster.ConsumeFromCollection(GetGoldSources(), (uint)cost))
				{
					base.OnCast();

					for (var i = 0; i < amount; i++)
						new HoodwinkOpal().Move(target, true, _caster.Segment);

					_caster.EmitSound(233, 3, 6);

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
			 * If the next tile does not allow pathing through,
			 * we interrupt the path. This will cause the spell the fizzle. */
			if (_caster.GetDistanceToMax(next) > Range || !AllowCastAt(segmentTile))
			{
				Fizzle();
				FinishSequence();
				return;
			}
				
			target = next;
		}
			
		CastAt(target);
	}

	private IEnumerable<Gold> GetGoldSources()
	{
		/* Search the hands. */
		var rightHand = _caster.RightHand;
		var leftHand = _caster.LeftHand;
		var backpack = _caster.Backpack;

		if (rightHand is Gold rightHandGold)
			yield return rightHandGold;
			
		if (leftHand is Gold leftHandGold)
			yield return leftHandGold;
			
		/* Search backpack. */
		if (backpack != null && backpack.OfType<Gold>().FirstOrDefault() is Gold backpackGold)
			yield return backpackGold;
			
		/* Search ground. */
		var segment = _caster.Segment;
		var location = _caster.Location;

		if (segment.GetItemsAt(location).OfType<Gold>().FirstOrDefault() is Gold worldGold)
			yield return worldGold;
	}
		
	private class InternalTarget : Target
	{
		private HoodwinkSpell _spell;

		public InternalTarget(HoodwinkSpell spell) : base(HoodwinkSpell.Range, TargetFlags.Path)
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

public class HoodwinkOpal : ItemEntity, ITreasure
{
	/// <inheritdoc />
	public override int LabelNumber => 6000100;
		
	/// <inheritdoc />
	public override int Category => 3;
		
	/// <inheritdoc />
	public override bool CanDisintegrate => true;

	public HoodwinkOpal() : base(392)
	{
		IsConjured = true;
	}

	public HoodwinkOpal(Serial serial) : base(serial)
	{
	}
		
	/// <summary>
	/// Serializes this instance into binary data for persistence.
	/// </summary>
	public override void Serialize(SpanWriter writer)
	{
		base.Serialize(writer);

		writer.Write((short)1); /* version */
	}

	/// <summary>
	/// Deserializes this instance from persisted binary data.
	/// </summary>
	public override void Deserialize(ref SpanReader reader)
	{
		base.Deserialize(ref reader);

		var version = reader.ReadInt16();

		switch (version)
		{
			case 1:
			{
				break;
			}
		}
	}

	public override bool OnDragLift(MobileEntity entity)
	{
		if (!base.OnDragLift(entity))
			return false;

		entity.SendLocalizedMessage(6300376); /* The dull opal crumbles as you lift it. */

		Delete();
		return false;
	}
}
