using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class ShadowstepSpell : InstantSpell, IWorldSpell
{
	private static SpellInfo _info = new SpellInfo(93, "Shadowstep", typeof(ShadowstepSpell), 12);
		
	public override string Name => _info.Name;
	
	protected override void OnCast()
	{
		_caster.Target = new InternalTarget(this);
	}

	public bool AllowCastAt(SegmentTile segmentTile)
	{
		return segmentTile.AllowsSpellPath(_caster, this) &&
		       (segmentTile.ContainsComponent<Floor>() 
		        || segmentTile.ContainsComponent<Staircase>());
	}

	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		var segment = _caster.Segment;
		var casterLocation = _caster.Location;
			
		/* Spell can not be cast on the caster's location. */
		var validLocation = (_caster is not PlayerEntity || target != casterLocation);
			
		if (CheckSequence() && validLocation)
		{
			var segmentTile = segment.FindTile(target);

			if (segmentTile != null && AllowCastAt(segmentTile))
			{
				base.OnCast();
					
				_caster.EmitSound(233, 3, 6);
				_caster.Teleport(target);
					
				var spellPower = Math.Min(16, (16 + (_skillLevel - 14) * 2));
				var hostiles = segmentTile.GetGroupsInRange(1).OfType<CreatureGroup>();
					
				// TODO: What to do about PvP? This only applies to creatures.
				// We should resolve all entities, then check if we have been hostile to them.
				foreach (var creature in hostiles.SelectMany(g => g.Members))
				{
					var distance = creature.GetDistanceToMax(target);
					var rounds = (int)(spellPower / (distance + 1));
						
					if (!creature.GetStatus(typeof(ShadowstepStatus), out var status))
					{
						status = new ShadowstepStatus(creature, rounds)
						{
							Inscription = new SpellInscription() { SpellId = _info.SpellId, }
						};
						status.AddSource(new SpellSource(_caster, TimeSpan.Zero));

						creature.AddStatus(status);
					}
					else
					{
						status.AddSource(new SpellSource(_caster, TimeSpan.Zero));
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
		var segment = _caster.Segment;
		var target = _caster.Location;

		foreach (var direction in directions)
		{
			var next = target + direction;

			if (_caster.GetDistanceToMax(next) > 3)
			{
				Fizzle();
				FinishSequence();
				return;
			}

			var segmentTile = segment.FindTile(next);

			if (segmentTile == null)
			{
				target = next;
				continue;
			}
				
			if (segmentTile.GetComponent<Wall>() is { IsIndestructible: true } || (segmentTile.ContainsComponent<Obstruction>()) 
			                                                                   || (segmentTile.ContainsComponent<Counter>()) 
			                                                                   || (segmentTile.ContainsComponent<Altar>()))
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
			Warm(source);
			CastAt(directions);
			return true;
		}

		return false;
	}
		
	private class InternalTarget : Target
	{
		private ShadowstepSpell _spell;
			
		public InternalTarget(ShadowstepSpell spell) : base(3, TargetFlags.Path | TargetFlags.Direction)
		{
			_spell = spell;
		}

		protected override void OnPath(MobileEntity source, List<Direction> path)
		{
			if (source.Spell != _spell)
				return;
				
			_spell.CastAt(path);
		}
			
		protected override void OnTargetCancel(MobileEntity source, TargetCancel cancelType)
		{
			base.OnTargetCancel(source, cancelType);

			if (source.Spell != _spell)
				return;
				
			_spell.Cancel();
		}
	}
}

