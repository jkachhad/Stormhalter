using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Kesmai.WorldForge.Editor;

public class SegmentTile : ObservableObject, IEnumerable<TerrainComponent>
{
	private int _x;
	private int _y;
		
	private List<TerrainRender> _renders;

	public int X => _x;
	public int Y => _y;

	public ObservableCollection<TerrainComponent> Components { get; set; }

	public List<TerrainRender> Renders => _renders;

	public SegmentTile(int x, int y)
	{
		_x = x;
		_y = y;

		Components = new ObservableCollection<TerrainComponent>();
	}
		
	public SegmentTile(XElement element)
	{
		_x = (int)element.Attribute("x");
		_y = (int)element.Attribute("y");
			
		Components = new ObservableCollection<TerrainComponent>();

		foreach (var componentElement in element.Elements("component"))
		{
			var component = InstantiateComponent(componentElement);

			if (component != null)
				Components.Add(component);
		}
			
		UpdateTerrain();
	}
		
	/// <summary>
	/// Gets an XML element that describes this instance.
	/// </summary>
	public XElement GetXElement()
	{
		var tileElement = new XElement("tile");

		foreach (var component in Components)
			tileElement.Add(component.GetXElement());

		return tileElement;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <inheritdoc />
	public IEnumerator<TerrainComponent> GetEnumerator()
	{
		return Components.GetEnumerator();
	}

	public IEnumerable<ComponentRender> GetRenderableTerrain(TerrainSelector selector)
	{
		foreach (var component in Components)
		{
			if (selector.IsValid(component))
			{
				foreach (var render in component.GetTerrain())
					yield return selector.TransformRender(this, component, render);
			}
		}
	}

	public void AddComponent(TerrainComponent component)
	{
		if (component != null)
			Components.Add(component);

		UpdateTerrain();
	}

	public void RemoveComponent(TerrainComponent component)
	{
		if (component != null && Components.Contains(component))
			Components.Remove(component);

		UpdateTerrain();
	}

	public void InsertComponent(int index, TerrainComponent component)
	{
		if (component != null)
			Components.Insert(index, component);

		UpdateTerrain();
	}

	public void ReplaceComponent(int index, TerrainComponent overwrite)
	{
		if (overwrite == null)
			return;

		Components.RemoveAt(index);
		Components.Insert(index, overwrite);

		UpdateTerrain();
	}

	public void ReplaceComponent(TerrainComponent original, TerrainComponent overwrite)
	{
		if (overwrite == null || original == null || !Components.Contains(original))
			return;

		var index = Components.IndexOf(original);

		Components.Remove(original);
		Components.Insert(index, overwrite);

		UpdateTerrain();
	}
		
	private TerrainComponent InstantiateComponent(XElement element)
	{
		var type = typeof(StaticComponent);
		var typeAttribute = element.Attribute("type");

		if (typeAttribute != null)
			type = Type.GetType(String.Format("Kesmai.WorldForge.Models.{0}", (string)typeAttribute));

		if (type == null)
			throw new ArgumentNullException("Unable to find the specified component type");

		var ctor = type.GetConstructor(new Type[] { typeof(XElement) });
		var component = ctor.Invoke(new object[] { element }) as TerrainComponent;

		return component;
	}

	public IEnumerable<T> GetComponents<T>()
	{
		return Components.OfType<T>().ToArray<T>();
	}

	public IEnumerable<T> GetComponents<T>(Func<T, bool> predicate)
	{
		return Components.OfType<T>().Where<T>(predicate).ToArray<T>();
	}

	public void UpdateTerrain()
	{
		var presenter = ServiceLocator.Current.GetInstance<ApplicationPresenter>();

		if (presenter != null)
			UpdateTerrain(presenter.SelectedFilter);
	}
		
	public void UpdateTerrain(TerrainSelector selector)
	{
		var componentRenders = GetRenderableTerrain(selector);
		var renders = new List<TerrainRender>();

		foreach (var render in componentRenders)
			renders.AddRange(render.Terrain.Select(layer => new TerrainRender(layer, render.Color)));

		_renders = renders.OrderBy(render => render.Layer.Order).ToList();
	}
}