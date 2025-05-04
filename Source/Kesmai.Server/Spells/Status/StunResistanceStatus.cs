using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StunResistanceStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	public StunResistanceStatus(MobileEntity entity) : base(entity)
	{
	}
		
	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.Stats[EntityStat.StunResistance].Add(+6, ModifierType.Constant);
	}

	public override void OnRemoved()
	{
		_entity.Stats[EntityStat.StunResistance].Remove(+6, ModifierType.Constant);

		base.OnRemoved();
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 551); /* The spell of [Stun Resistance] has worn off. */
		}
	}
}