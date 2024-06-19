using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("RuinsComponent")]
public class Ruins : TerrainComponent, IHandleMovement, IHandlePathing
{
	private Terrain _ruins;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	public Ruins(Terrain ruins)
	{
		_ruins = ruins;
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Ruins"/> class.
	/// </summary>
	public Ruins(XElement element) : base(element)
	{
		if (element.TryGetElement("ruins", out var ruinsElement))
			_ruins = Terrain.Get((int)ruinsElement, Color);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_ruins != null)
			yield return _ruins;
	}
		
	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return true;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles movement over this component.
	/// </summary>
	public void HandleMovementPath(PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Interrupted;
	}

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => 3;
		
	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public virtual void OnEnter(MobileEntity entity, bool isTeleport)
	{
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(MobileEntity entity, bool isTeleport)
	{
	}
}