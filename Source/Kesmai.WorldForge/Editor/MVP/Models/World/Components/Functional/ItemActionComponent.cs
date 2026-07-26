using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using CommonServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Models;

public class ItemActionComponent : TerrainComponent
{
	private static ComponentRender _internal;
	
	static ItemActionComponent()
	{
		var contentManager = ServiceLocator.Current.GetInstance<ContentManager>();
			
		var terrain = new Terrain(
			new TerrainLayer()
			{
				Sprite = new GameSprite(contentManager.Load<Texture2D>(@"WorldForge/Terrain/Teleporter")),
				Order = 9
			}
		);

		_internal = new ComponentRender(terrain, Color.FromNonPremultiplied(150, 255, 255, 150));
	}
	
	private string _tag;
	
	private string _itemAddedAction;
	private string _itemRemovedAction;

	private bool _ignoreTeleports;
	
	[Browsable(true)]
	public string Tag
	{
		get => _tag;
		set => _tag = value;
	}
	
	[Browsable(true)]
	public string ItemAdded
	{
		get => _itemAddedAction;
		set => _itemAddedAction = value;
	}
	
	[Browsable(true)]
	public string ItemRemoved
	{
		get => _itemRemovedAction;
		set => _itemRemovedAction = value;
	}
	
	[Browsable(true)]
	public bool IgnoreTeleports
	{
		get => _ignoreTeleports;
		set => _ignoreTeleports = value;
	}
	
	public ItemActionComponent(XElement element) : base(element)
	{
		if (element.TryGetElement("tag", out var tagElement))
			_tag = tagElement.Value;

		if (element.TryGetElement("itemAdded", out var itemAddedElement))
			_itemAddedAction = itemAddedElement.Value;

		if (element.TryGetElement("itemRemoved", out var itemRemovedElement))
			_itemRemovedAction = itemRemovedElement.Value;
		
		if (element.TryGetElement("ignoreTeleports", out var ignoreTeleportsElement))
			_ignoreTeleports = bool.Parse(ignoreTeleportsElement.Value);
	}
	
	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		yield return _internal;
	}
	
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		if (!String.IsNullOrEmpty(_tag))
			element.Add(new XElement("tag", _tag));
		
		if (!String.IsNullOrEmpty(_itemAddedAction))
			element.Add(new XElement("itemAdded", _itemAddedAction));
		
		if (!String.IsNullOrEmpty(_itemRemovedAction))
			element.Add(new XElement("itemRemoved", _itemRemovedAction));
		
		if (_ignoreTeleports)
			element.Add(new XElement("ignoreTeleports", _ignoreTeleports));
		
		return element;
	}

	public override TerrainComponent Clone()
	{
		return new ItemActionComponent(GetSerializingElement());
	}
}