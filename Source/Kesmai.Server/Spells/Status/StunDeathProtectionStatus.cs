using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class StunDeathProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	public StunDeathProtectionStatus(MobileEntity entity) : base(entity)
	{
	}

	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.Stats[EntityStat.StunProtection].Add(+1, ModifierType.Constant);
		_entity.Stats[EntityStat.DeathProtection].Add(+1, ModifierType.Constant);
	}

	public override void OnRemoved()
	{
		_entity.Stats[EntityStat.StunProtection].Remove(+1, ModifierType.Constant);
		_entity.Stats[EntityStat.DeathProtection].Remove(+1, ModifierType.Constant);
			
		base.OnRemoved();
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 545); /* The spell of [Protection from Stun and Death] has worn off. */
		}
	}
}