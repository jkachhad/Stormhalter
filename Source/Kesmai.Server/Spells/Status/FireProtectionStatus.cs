using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class FireProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 221;
		
	public FireProtectionStatus(MobileEntity entity) : base(entity)
	{
	}

	public override void OnAcquire()
	{
		base.OnAcquire();
			
		_entity.Stats[EntityStat.FireProtection].Add(+20, ModifierType.Constant);
	}

	public override void OnRemoved()
	{
		_entity.Stats[EntityStat.FireProtection].Remove(+20, ModifierType.Constant);
			
		base.OnRemoved();
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 543); /* The spell of [Protection from Fire] has worn off. */
		}
	}
}