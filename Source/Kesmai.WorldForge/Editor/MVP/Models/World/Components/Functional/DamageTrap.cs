using System.ComponentModel;
using System.Xml.Linq;

namespace Kesmai.WorldForge.Models;

public abstract class DamageTrap : TrapComponent
{
	private int _damage;
	
	[Browsable(true)]
	public int Damage
	{
		get => _damage;
		set => _damage = value;
	}

	protected DamageTrap(XElement element) : base(element)
	{
		var damageElement = element.Element("damage");

		if (damageElement != null)
			_damage = (int)damageElement;
	}
	
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (_damage > 0)
			element.Add(new XElement("damage", _damage));
		
		return element;
	}
}