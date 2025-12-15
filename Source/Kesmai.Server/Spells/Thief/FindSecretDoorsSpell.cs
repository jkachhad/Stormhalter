using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class FindSecretDoorsSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(16, "Find Secret Doors", typeof(FindSecretDoorsSpell), 4);
		
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
		if (!_caster.IsAlive || !_caster.CanPerformAction)
			return;
			
		if (CheckSequence())
		{
			base.OnCast();
				
			var segment = _caster.Segment;

			var secretDoors = segment.GetComponentsInLOS<Door>(_caster).Where(r => r.Component.IsSecret).ToList();
			var portals = segment.GetComponentsInLOS<Portal>(_caster).ToList();

			if (portals.Count is not 0)
				portals.ForEach(result => result.Component.Dispel(result.SegmentTile));

			if (secretDoors.Count > 0)
			{
				foreach (var (segmentTile, door) in secretDoors)
					door.Open(segmentTile);

				if (_caster is PlayerEntity player && _item == null)
					player.AwardMagicSkill(this);
			}
			else
			{
				_caster.SendLocalizedMessage(6300358); /* You could not detect any secret doors. */
			}
				
			_caster.EmitSound(233, 3, 6);
		}
		else
		{
			Fizzle();
		}

		FinishSequence();
	}
}
