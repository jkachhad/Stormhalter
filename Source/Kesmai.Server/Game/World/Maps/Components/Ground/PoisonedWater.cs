using System;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("PoisonedWaterComponent")]
public class PoisonedWater : Water
{
	private int _potency;
		
	public PoisonedWater(XElement element) : base(element)
	{
		if (element.TryGetElement("potency", out var potencyElement))
			_potency = (int)potencyElement;
		else
			_potency = 3;
	}

	public override void OnEnter(MobileEntity entity)
	{
		base.OnEnter(entity);
			
		/* Poison the entity. */
		entity.Poison(null, new Poison(TimeSpan.Zero, _potency));
			
		/* Check for any open bottles in their hands. */
		var hands = entity.Hands;

		if (hands is null)
			return;
			
		var bottles = hands.OfType<Bottle>().Where(b => b.IsOpen);

		foreach (var bottle in bottles)
		{
			bottle.Content = new ConsumablePoison(_potency);
			bottle.Owner = entity;
		}
	}
}