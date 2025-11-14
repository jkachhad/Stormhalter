using System.Collections.Generic;
using System.Linq;
using Kesmai.Server.Game;
using Kesmai.Server.Targeting;

namespace Kesmai.Server.Spells;

public class CloseOpenDoorSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(10, "Close/Open Door", typeof(CloseOpenDoorSpell), 3);
		
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

	public void CastAt(Point2D target)
	{
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;

		if (CheckSequence())
		{
			var segment = _caster.Segment;
			var segmentTile = segment.FindTile(target);

			// TODO: Locked doors can not be opened through this spell.
			if (segmentTile != null && segmentTile.GetComponent<Door>() is Door door && !door.IsSecret)
			{
				base.OnCast();
					
				door.Toggle(segmentTile);

				segment.PlaySound(target, 232, 3, 6);

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
		private CloseOpenDoorSpell _spell;
			
		public InternalTarget(CloseOpenDoorSpell spell) : base(7, TargetFlags.Path)
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