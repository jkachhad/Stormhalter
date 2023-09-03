using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("ObstructionComponent")]
public class Obstruction : TerrainComponent, IHandleVision, IHandlePathing
{
	private Terrain _obstruction;
	private bool _blocksVision;

	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	public bool BlocksVision
	{
		get { return _blocksVision; }
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Obstruction"/> class.
	/// </summary>
	public Obstruction(int obstructionId, bool blocksVision) : base()
	{
		_obstruction = Terrain.Get((int)obstructionId, Color);
		_blocksVision = blocksVision;
	}
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Obstruction"/> class.
	/// </summary>
	public Obstruction(XElement element) : base(element)
	{
		if (element.TryGetElement("obstruction", out var obstructionElement))
			_obstruction = Terrain.Get((int)obstructionElement, Color);
			
		if (element.TryGetElement("blockVision", out var blockVisionElement))
			_blocksVision = (bool)blockVisionElement;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_obstruction != null)
			yield return _obstruction;
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return false;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public virtual void HandleMovementPath(PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Rejected;
	}
}