using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using CommonServiceLocator;

namespace Kesmai.WorldForge.Models;

public class Fire : TerrainComponent
{
	private bool _allowDispel;
	private int _damage = 3;
		
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
		
	public Fire(int damage, bool allowDispel)
	{
		_damage = damage;
		_allowDispel = allowDispel;
	}
		
	public Fire(XElement element) : base(element)
	{
		var damageElement = element.Element("damage");

		if (damageElement != null)
			_damage = (int)damageElement;
		
		var allowDispel = element.Element("allowDispel");

		if (allowDispel != null)
			_allowDispel = (bool)allowDispel;
	}
		
	public override IEnumerable<ComponentRender> GetTerrain()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (terrainManager.TryGetValue(135, out Terrain terrain))
			yield return new ComponentRender(terrain, Color);
	}
		
	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (_damage > 0 && _damage != 3)
			element.Add(new XElement("damage", _damage));
		
		if (_allowDispel)
			element.Add(new XElement("allowDispel", _allowDispel));

		return element;
	}

	public override TerrainComponent Clone()
	{
		return new Fire(GetXElement());
	}
}