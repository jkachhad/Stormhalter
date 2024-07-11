using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("SpeechTeleporter")]
public class SpeechTeleporter : HiddenTeleporter, IHandleSpeech
{
	protected Terrain _teleporter;
		
	public string Phrase { get; set; }
		
	public SpeechTeleporter(XElement element) : base(element)
	{
		if (element.TryGetElement("teleporterId", out var teleporterIdElement))
			_teleporter = Terrain.Get((int)teleporterIdElement, Color);
			
		if (element.TryGetElement("phrase", out var phraseElement))
			Phrase = phraseElement.Value;
	}
		
	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (_teleporter != null)
			yield return _teleporter;
	}
		
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public override void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;
	}

	public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		/* We do nothing when a player enters this teleporter. We wait until a phrase is said. */
	}

	/// <summary>
	/// Handles speech provided by an entity.
	/// </summary>
	public bool HandleSpeech(SegmentTile parent, MobileEntity entity, string phrase)
	{
		if (phrase.Matches(Phrase, true))
		{
			if (!base.CanTeleport(parent, entity))
				return false;

			Teleport(parent, entity);

			entity.QueueMovementTimer();
			return true;
		}

		return false;
	}
}