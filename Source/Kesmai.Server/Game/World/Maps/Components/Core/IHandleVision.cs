namespace Kesmai.Server.Game;

public interface IHandleVision
{
	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	bool BlocksVision { get; }
}