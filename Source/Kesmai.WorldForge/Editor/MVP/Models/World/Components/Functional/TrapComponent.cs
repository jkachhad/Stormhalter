using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using CommonServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Models;

public abstract class TrapComponent : TerrainComponent
{
	private static ComponentRender _internal;
	
	static TrapComponent()
	{
		var contentManager = ServiceLocator.Current.GetInstance<ContentManager>();
			
		var terrain = new Terrain(
			new TerrainLayer()
			{
				Sprite = new GameSprite(contentManager.Load<Texture2D>(@"WorldForge/Terrain/Teleporter")),
				Order = 9
			}
		);

		_internal = new ComponentRender(terrain, Color.FromNonPremultiplied(0, 255, 255, 150));
	}
	
	private int _cooldown;
	
	private bool _trapCreatures;
	private bool _trapPlayer;

	private bool _interruptMovement;

	[Browsable(true)]
	public int Cooldown
	{
		get => _cooldown;
		set => _cooldown = value;
	}

	[Browsable(true)]
	public bool TrapCreatures
	{
		get => _trapCreatures;
		set => _trapCreatures = value;
	}

	[Browsable(true)]
	public bool TrapPlayers
	{
		get => _trapPlayer;
		set => _trapPlayer = value;
	}
	
	[Browsable(true)]
	public bool InterruptMovement
	{
		get => _interruptMovement;
		set => _interruptMovement = value;
	}
	
	public TrapComponent(XElement element) : base(element)
	{
		var cooldownElement = element.Element("cooldown");

		if (cooldownElement != null)
			_cooldown = (int)cooldownElement;
		
		var trapCreaturesElement = element.Element("trapCreatures");
		var trapPlayerElement = element.Element("trapPlayer");
		var interruptMovementElement = element.Element("interruptMovement");

		if (trapCreaturesElement != null)
			_trapCreatures = (bool)trapCreaturesElement;
		else
			_trapCreatures = true;

		if (trapPlayerElement != null)
			_trapPlayer = (bool)trapPlayerElement;

		if (interruptMovementElement != null)
			_interruptMovement = (bool)interruptMovementElement;
	}
	
	public override IEnumerable<ComponentRender> GetRenders()
	{
		yield return _internal;
	}

	public override XElement GetXElement()
	{
		var element = base.GetXElement();

		if (_cooldown > 0)
			element.Add(new XElement("cooldown", _cooldown));

		if (!_trapCreatures)
			element.Add(new XElement("trapCreatures", _trapCreatures));
		
		if (_trapPlayer)
			element.Add(new XElement("trapPlayer", _trapPlayer));

		if (_interruptMovement)
			element.Add(new XElement("interruptMovement", _interruptMovement));
		
		return element;
	}
}