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
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_teleporter != null)
			yield return _teleporter;
	}
		
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public override void HandleMovementPath(PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;
	}

	public override void OnEnter(MobileEntity entity, bool isTeleport)
	{
		/* We do nothing when a player enters this teleporter. We wait until a phrase is said. */
	}

	/// <summary>
	/// Handles speech provided by an entity.
	/// </summary>
	public bool HandleSpeech(MobileEntity entity, string phrase)
	{
		if (phrase.Matches(Phrase, true))
		{
			if (!base.CanTeleport(entity))
				return false;

			Timer.DelayCall(() => Teleport(entity));

			entity.QueueMovementTimer();
			return true;
		}

		return false;
	}
}