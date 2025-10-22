using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using CommonServiceLocator;

namespace Kesmai.WorldForge.Models;

public class Whirlwind : TerrainComponent
{
	private bool _allowDispel;
	private int _damage;
		
	[Browsable(true)]
	public bool AllowDispel
	{
		get => _allowDispel;
		set => _allowDispel = value;
	}
		
	[Browsable(true)]
	public int Damage
	{
		get => _damage;
		set => _damage = value;
	}
		
	public Whirlwind(int damage, bool allowDispel)
	{
		_damage = damage;
		_allowDispel = allowDispel;
	}
		
	public Whirlwind(XElement element) : base(element)
	{
		var damageElement = element.Element("damage");

		if (damageElement != null)
			_damage = (int)damageElement;
			
		var allowDispelElement = element.Element("allowDispel");

		if (allowDispelElement != null)
			_allowDispel = (bool)allowDispelElement;
	}
		
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(132, out Terrain terrain))
			yield return new ComponentRender(terrain, Color);
	}
		
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (_damage > 0)
			element.Add(new XElement("damage", _damage));
			
		if (_allowDispel)
			element.Add(new XElement("allowDispel", _allowDispel));

		return element;
	}

	public override TerrainComponent Clone()
	{
		return new Whirlwind(GetXElement());
	}
}