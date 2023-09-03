namespace Kesmai.Server.Game;

public interface IDestructable
{
	/// <summary>
	/// Gets a value indicating whether this terrain is destroyed.
	/// </summary>
	bool IsDestroyed { get; }
}