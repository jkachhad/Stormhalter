using System;
using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class RapidfireStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;

	public bool IsActive => true;
		
	public RapidfireStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 598); /* The spell of [Speed] has worn off. */
		}
	}
}