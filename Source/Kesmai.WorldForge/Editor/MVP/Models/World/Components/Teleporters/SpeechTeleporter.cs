using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using CommonServiceLocator;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Kesmai.WorldForge.Models;

public class SpeechTeleporter : HiddenTeleporterComponent
{
	private static ComponentRender _internal;
		
	static SpeechTeleporter()
	{
		var contentManager = ServiceLocator.Current.GetInstance<ContentManager>();
			
		var terrain = new Terrain(
			new TerrainLayer()
			{
				Sprite = new GameSprite(contentManager.Load<Texture2D>(@"WorldForge/Terrain/Teleporter")),
				Order = 9
			}
		);

		_internal = new ComponentRender(terrain, Color.FromNonPremultiplied(0, 255, 0, 150));
	}
		
	private int _teleporterId;

	[Browsable(true)]
	public int TeleporterId
	{
		get => _teleporterId;
		set => _teleporterId = value;
	}
		
	[Browsable(true)]
	public string Phrase { get; set; }
		
	public SpeechTeleporter(XElement element) : base(element)
	{
		var teleporterIdElement = element.Element("teleporterId");

		if (teleporterIdElement != null)
			_teleporterId = Int32.Parse(teleporterIdElement.Value);
			
		var phraseElement = element.Element("phrase");
			
		if (phraseElement != null)
			Phrase = (string)phraseElement;
	}	
		
	/// <inheritdoc />
	public override IEnumerable<ComponentRender> GetRenders()
	{
		var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

		if (_teleporterId > 0)
		{
			if (terrainManager.TryGetValue(_teleporterId, out Terrain terrain))
				yield return new ComponentRender(terrain, (IsValid() ? Color : Color.Red));
		}
		else
		{
			yield return _internal;
		}
	}
		
	public override XElement GetSerializingElement()
	{
		var element = base.GetSerializingElement();

		element.Add(new XElement("teleporterId", _teleporterId));

		if (!String.IsNullOrEmpty(Phrase))
			element.Add(new XElement("phrase", Phrase));
	
		return element;
	}
		
	public override TerrainComponent Clone()
	{
		return new SpeechTeleporter(GetSerializingElement());
	}
}