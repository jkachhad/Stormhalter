using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("DoorComponent")]
public class Door : TerrainComponent, IHandleVision, IHandlePathing, IHandleMovement, IHandleInteraction
{
	private static Dictionary<SegmentTile, Timer> _closeTimers = new Dictionary<SegmentTile, Timer>();
	private static Dictionary<SegmentTile, Timer> _hideTimers = new Dictionary<SegmentTile, Timer>();

	private static void StartCloseTimer(SegmentTile parent, Door component, TimeSpan duration)
	{
		if (_closeTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_closeTimers[parent] = Timer.DelayCall(duration, () => component.Close(parent));
	}

	private static void StopCloseTimer(SegmentTile parent)
	{
		if (_closeTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_closeTimers.Remove(parent);
	}
	
	private static void StartHideTimer(SegmentTile parent, Door component, TimeSpan duration)
	{
		if (_hideTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_hideTimers[parent] = Timer.DelayCall(duration, () => component.Unhide(parent));
	}

	private static void StopHideTimer(SegmentTile parent)
	{
		if (_hideTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_hideTimers.Remove(parent);
	}
	
	/// <summary>
	/// Gets the delay after which secret doors are automatically closed.
	/// </summary>
	private static TimeSpan SecretDoorCloseDelay = TimeSpan.FromMinutes(1.0);

	private Terrain _openDoor;
	private Terrain _closedDoor;
	private Terrain _secretDoor;
	private Terrain _destroyedDoor;

	private bool _isSecret;
	private bool _isOpen;
	private bool _isDestroyed;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	/// <summary>
	/// Gets a value indicating whether this instance blocks line-of-sight.
	/// </summary>
	public bool BlocksVision => !IsOpen;

	/// <summary>
	/// Gets or sets a value indicating whether this door is secret.
	/// </summary>
	public bool IsSecret => _isSecret;

	/// <summary>
	/// Gets or sets a value indicating whether this door is open.
	/// </summary>
	public bool IsOpen => _isOpen || IsDestroyed;

	/// <summary>
	/// Gets or sets a value indicating whether this door is destroyed.
	/// </summary>
	public bool IsDestroyed => _isDestroyed;
		
	/// <summary>
	/// Gets a value indicating whether this instance is indestructible.
	/// </summary>
	public bool IsIndestructible { get; private set; }
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Door"/> class.
	/// </summary>
	public Door(XElement element) : base(element)
	{
		if (element.TryGetElement("openId", out var openIdElement))
			_openDoor = Terrain.Get((int)openIdElement, Color);
			
		if (element.TryGetElement("closedId", out var closedIdElement))
			_closedDoor = Terrain.Get((int)closedIdElement, Color);
			
		if (element.TryGetElement("secretId", out var secretIdElement))
			_secretDoor = Terrain.Get((int)secretIdElement, Color);
			
		if (element.TryGetElement("destroyedId", out var destroyedIdElement))
			_destroyedDoor = Terrain.Get((int)destroyedIdElement, Color);
			
		if (element.TryGetElement("isOpen", out var isOpenElement))
			_isOpen = (bool)isOpenElement;
			
		if (element.TryGetElement("isSecret", out var isSecretElement))
			_isSecret = (bool)isSecretElement;
			
		if (element.TryGetElement("isDestroyed", out var isDestroyedElement))
			_isDestroyed = (bool)isDestroyedElement;
			
		if (element.TryGetElement("indestructible", out var indestructibleElement))
			IsIndestructible = (bool)indestructibleElement;
	}
		
	/// <summary>
	/// Gets the terrain values from this component.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		if (!IsDestroyed)
		{
			var openId = _openDoor;
			var closedId = _closedDoor;

			if (_isSecret || IsHidden(parent))
				closedId = _secretDoor;

			yield return (_isOpen ? openId : closedId);
		}
		else
		{
			yield return _destroyedDoor;
		}
	}

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => (!_isOpen ? 3 : 1);
		
	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		if (!_isOpen)
			Open(parent);
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return (IsOpen || !IsSecret);
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity),
		Spell spell = default(Spell))
	{
		if (IsOpen || spell is FindSecretDoorsSpell)
			return true;

		return false;
	}
		
	/// <summary>
	/// Handles movement over this component.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		if (!IsOpen)
			args.Result = (_isSecret ? PathingResult.Daze : PathingResult.Interrupted);
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(SegmentTile parent, MobileEntity entity, ActionType action)
	{
		if (IsDestroyed)
			return false;

		if (action != ActionType.CloseDoor && action != ActionType.OpenDoor && action != ActionType.Search)
			return false;
			
		if (_isOpen)
		{
			if (action == ActionType.CloseDoor && CanClose(parent))
				Close(parent);
		}
		else
		{
			var secret = _isSecret || IsHidden(parent);
				
			if ((secret && action == ActionType.Search) || (action == ActionType.OpenDoor))
				Open(parent);
		}
			
		entity.QueueRoundTimer();
		return true;
	}

	private void Delta(SegmentTile parent)
	{
		parent.Delta(TileDelta.Terrain);
		parent.UpdateFlags();
	}

	/// <summary>
	/// Closes this instance.
	/// </summary>
	public void Close(SegmentTile parent)
	{
		if (_isOpen && CanClose(parent))
		{
			_isOpen = false;

			parent.PlaySound(74, 3, 6);

			Delta(parent);
				
			StopCloseTimer(parent);
		}
		else
		{
			StartCloseTimer(parent, this, SecretDoorCloseDelay);
		}
	}

	public void Open(SegmentTile parent, bool silent = false)
	{
		if (IsOpen)
			return;
			
		_isOpen = true;

		Unhide(parent);

		if (!silent)
			parent.PlaySound(73, 3, 6);
			
		Delta(parent);

		if (IsHidden(parent))
			StopHideTimer(parent);
			
		if (_isSecret)
			StartCloseTimer(parent, this, SecretDoorCloseDelay);
	}

	private bool CanClose(SegmentTile parent)
	{
		if (parent is null || IsDestroyed)
			return false;
			
		if (parent.Groups.Any())
			return false;

		if (parent.OfType<Web>().Any())
			return false;

		return true;
	}
		
	public void Toggle(SegmentTile parent)
	{
		if (_isOpen)
			Close(parent);
		else
			Open(parent);
	}

	public bool Hide(SegmentTile parent, int skillLevel = 0)
	{
		// TODO: This door can't be destroyed.
		// TODO: This door needs 2 cardinal walls near it (horizontal/vertical).

		if (_isOpen)
		{
			if (!CanClose(parent))
				return false;

			Close(parent);
		}

		StartHideTimer(parent, this, parent.Facet.TimeSpan.FromRounds(30 + 2 * skillLevel));
			
		Delta(parent);
			
		return true;
	}

	public void Unhide(SegmentTile parent)
	{
		if (!IsHidden(parent))
			return;
			
		StopHideTimer(parent);
			
		Delta(parent);
	}
	
	public bool IsHidden(SegmentTile parent)
	{
		if (_hideTimers.TryGetValue(parent, out var timer) && timer.Running)
			return true;

		return false;
	}

	public void Destroy(SegmentTile parent)
	{
		if (IsIndestructible || IsDestroyed)
			return;
			
		Open(parent);
			
		_isDestroyed = true;
	}
}