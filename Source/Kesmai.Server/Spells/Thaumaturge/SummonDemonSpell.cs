using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class SummonDemonSpell : DelayedSpell
{
	private static SpellInfo _info = new SpellInfo(18, "Summon Demon", typeof(SummonDemonSpell), 14);
		
	public override string Name => _info.Name;
	
	protected override bool CheckSequence()
	{
		var segment = _caster.Segment;

		if (segment.GetSubregion(_caster.Location) is TownSubregion)
			return false;
			
		return base.CheckSequence();
	}
}