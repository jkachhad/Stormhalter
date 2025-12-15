using System.Drawing;
using System.Linq;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class PerceptionStatus : SpellStatus
{
	public override int SpellRemovedSound => 223;
		
	public PerceptionStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override void OnSourceAdded(SpellStatusSource source)
	{
		base.OnSourceAdded(source);
			
		_entity.Delta(MobileDelta.Visibility);
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		base.OnSourceRemoved(source);

		if (source is SpellSource && !_spellSources.Any())
		{
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 595); /* The spell of [Perception] has worn off. */
		}
			
		_entity.Delta(MobileDelta.Visibility);
	}
}