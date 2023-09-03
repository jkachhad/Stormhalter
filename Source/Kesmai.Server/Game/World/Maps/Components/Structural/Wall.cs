using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("WallComponent")]
public class Wall : TerrainComponent, IHandleInteraction, IHandleVision, IHandlePathing, IDestructable
{
	private Terrain _wall;
	private Terrain _destroyed;
	private Terrain _ruins;

	private bool _isDestroyed;

	/// <summary>
	/// Gets a value indicating whether this instance is indestructible.
	/// </summary>
	public bool IsIndestructible { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this terrain is destroyed.
	/// </summary>
	public bool IsDestroyed
	{
		get => _isDestroyed;
		set => _isDestroyed = value;
	}

	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	public bool BlocksVision => !_isDestroyed;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Wall"/> class.
	/// </summary>
	public Wall(XElement element) : base(element)
	{
		if (element.TryGetElement("wall", out var wallElement))
			_wall = Terrain.Get((int)wallElement, Color);
			
		if (element.TryGetElement("destroyed", out var destroyedElement))
			_destroyed = Terrain.Get((int)destroyedElement, Color);

		if (element.TryGetElement("ruins", out var ruinsElement))
			_ruins = Terrain.Get((int)ruinsElement, Color);

		if (element.TryGetElement("indestructible", out var indestructibleElement))
			IsIndestructible = (bool)indestructibleElement;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_isDestroyed)
		{
			if (_destroyed != null)
				yield return _destroyed;
		}
		else
		{
			if (_wall != null)
				yield return _wall;
		}
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (_isDestroyed || action != ActionType.Search)
			return false;

		entity.QueueRoundTimer();
		return true;
	}

	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return _isDestroyed;
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		if (_isDestroyed || spell is CreatePortalSpell)
			return true;
			
		return false;
	}
		
	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(PathingRequestEventArgs args)
	{
		/* If this terrain is destroyed, there should exist a ground component
		 * to provide a pathing result and movement cost. */
		if (!_isDestroyed)
			args.Result = PathingResult.Daze;
	}

	public void Destroy()
	{
		if (_isDestroyed || IsIndestructible)
			return;

		_isDestroyed = true;
			
		if (_ruins != null)
			_parent.Add(new Ruins(_ruins));
			
		_parent.Delta(TileDelta.Terrain);
	}
}