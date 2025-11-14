using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class BlindResistanceStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	public BlindResistanceStatus(MobileEntity entity) : base(entity)
	{
	}
		
	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.Stats[EntityStat.BlindResistance].Add(+6, ModifierType.Constant);
	}

	public override void OnRemoved()
	{
		_entity.Stats[EntityStat.BlindResistance].Remove(+6, ModifierType.Constant);

		base.OnRemoved();
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 547); /* The spell of [Blind Resistance] has worn off. */
		}
	}
}