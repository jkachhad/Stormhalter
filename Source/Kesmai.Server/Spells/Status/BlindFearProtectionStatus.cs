using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class BlindFearProtectionStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public BlindFearProtectionStatus(MobileEntity entity) : base(entity)
	{
	}

	public override void OnAcquire()
	{
		base.OnAcquire();

		_entity.Stats[EntityStat.BlindProtection].Add(+1, ModifierType.Constant);
		_entity.Stats[EntityStat.FearProtection].Add(+1, ModifierType.Constant);
	}

	public override void OnRemoved()
	{
		_entity.Stats[EntityStat.BlindProtection].Remove(+1, ModifierType.Constant);
		_entity.Stats[EntityStat.FearProtection].Remove(+1, ModifierType.Constant);

		base.OnRemoved();
	}
		
	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 541); /* The spell of [Protection from Blind and Fear] has worn off. */
		}
	}
}
