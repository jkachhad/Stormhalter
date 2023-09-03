using System;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("DamageTrap")]
public class DamageTrap : TrapComponent
{
	public int Damage { get; set; }
	
	public DamageTrap(XElement element) : base(element)
	{
		if (element.TryGetElement("damage", out var damageElement))
			Damage = (int)damageElement;
	}

	protected override void OnSpring(MobileEntity entity)
	{
		if (Damage > 0)
			entity.ApplyDamage(null, Damage);
	}
}