namespace Kesmai.Server.Game;

public interface IHandleSpeech
{
	bool HandleSpeech(MobileEntity entity, string phrase);
}