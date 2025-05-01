using System;
using System.Drawing;
using Kesmai.Server.Game;

namespace Kesmai.Server.Spells;

public class ShieldStatus : SpellStatus
{
	/// <summary>
	/// Gets the maximum protection provided by shield status.
	/// </summary>
	public static readonly int MaximumProtection = 9;
	
	public override int SpellRemovedSound => 221;

	public ShieldStatus(MobileEntity entity) : base(entity)
	{
	}

	protected override void OnSourceAdded(SpellStatusSource source)
	{
		base.OnSourceAdded(source);

		if (source is SpellSource spellSource && _spellSources.Count is 1)
			_entity.Stats[EntityStat.Barrier].Add(+3, ModifierType.Constant);
	}

	protected override void OnSourceRemoved(SpellStatusSource source)
	{
		if (source is SpellSource spellSource && _spellSources.Count is 0)
		{
			_entity.Stats[EntityStat.Barrier].Remove(+3, ModifierType.Constant);
			
			if (_entity.Client != null)
				_entity.SendLocalizedMessage(Color.Magenta, 6300270, 552); /* The spell of [Shield] has worn off. */
		}
		
		base.OnSourceRemoved(source);
	}
}