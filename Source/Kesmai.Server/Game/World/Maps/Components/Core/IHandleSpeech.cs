namespace Kesmai.Server.Game;

public interface IHandleSpeech
{
	bool HandleSpeech(SegmentTile parent, MobileEntity entity, string phrase);
}