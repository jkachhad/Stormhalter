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

	private Timer _closeTimer;
	private Timer _hideTimer;
	
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

	public bool IsHidden => _hideTimer != null && _hideTimer.Running;

	public bool IsOpen => _isOpen || IsDestroyed;

	public bool IsDestroyed
	{
		get => _isDestroyed;
		set => _isDestroyed = value;
	}
		
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
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (!IsDestroyed)
		{
			var openId = _openDoor;
			var closedId = _closedDoor;

			if (_isSecret || IsHidden)
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
	public void OnEnter(MobileEntity entity, bool isTeleport)
	{
		if (!_isOpen)
			Open();
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public void OnLeave(MobileEntity entity, bool isTeleport)
	{
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(MobileEntity entity = default(MobileEntity))
	{
		return (IsOpen || !IsSecret);
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		if (IsOpen || spell is FindSecretDoorsSpell)
			return true;

		return false;
	}
		
	/// <summary>
	/// Handles movement over this component.
	/// </summary>
	public void HandleMovementPath(PathingRequestEventArgs args)
	{
		if (!IsOpen)
			args.Result = (_isSecret ? PathingResult.Daze : PathingResult.Interrupted);
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (IsDestroyed)
			return false;

		if (action != ActionType.CloseDoor && action != ActionType.OpenDoor && action != ActionType.Search)
			return false;
			
		if (_isOpen)
		{
			if (action == ActionType.CloseDoor && CanClose())
				Close();
		}
		else
		{
			var secret = _isSecret || IsHidden;
				
			if ((secret && action == ActionType.Search) || (action == ActionType.OpenDoor))
				Open();
		}
			
		entity.QueueRoundTimer();
		return true;
	}

	private void Delta()
	{
		_parent.Delta(TileDelta.Terrain);
		_parent.UpdateFlags();
	}

	/// <summary>
	/// Closes this instance.
	/// </summary>
	public void Close()
	{
		if (_isOpen && CanClose())
		{
			_isOpen = false;

			_parent.PlaySound(74, 3, 6);

			Delta();
				
			if (_closeTimer != null && _closeTimer.Running)
				_closeTimer.Stop();

			_closeTimer = null;
		}
		else
		{
			if (_closeTimer != null && _closeTimer.Running)
				_closeTimer.Stop();

			_closeTimer = Timer.DelayCall(SecretDoorCloseDelay, Close);
		}
	}

	public void Open(bool silent = false)
	{
		if (IsOpen)
			return;
			
		_isOpen = true;

		Unhide();

		if (!silent)
			_parent.PlaySound(73, 3, 6);
			
		Delta();

		if (IsHidden)
		{
			if (_hideTimer != null && _hideTimer.Running)
				_hideTimer.Stop();

			_hideTimer = null;
		}
			
		if (_isSecret)
		{
			if (_closeTimer != null && _closeTimer.Running)
				_closeTimer.Stop();

			_closeTimer = Timer.DelayCall(SecretDoorCloseDelay, Close);
		}
	}

	private bool CanClose()
	{
		if (_parent is null || IsDestroyed)
			return false;
			
		if (_parent.Groups.Count > 0)
			return false;

		if (_parent.OfType<Web>().Any())
			return false;

		return true;
	}
		
	public void Toggle()
	{
		if (_isOpen)
			Close();
		else
			Open();
	}

	public bool Hide(int skillLevel = 0)
	{
		// TODO: This door can't be destroyed.
		// TODO: This door needs 2 cardinal walls near it (horizontal/vertical).

		if (_isOpen)
		{
			if (!CanClose())
				return false;

			Close();
		}

		if (_hideTimer != null && _hideTimer.Running)
			_hideTimer.Stop();

		_hideTimer = Timer.DelayCall(_parent.Facet.TimeSpan.FromRounds(30 + 2 * skillLevel), Unhide);
			
		Delta();
			
		return true;
	}

	public void Unhide()
	{
		if (!IsHidden)
			return;
			
		if (_hideTimer != null && _hideTimer.Running)
			_hideTimer.Stop();
			
		Delta();
	}

	public void Destroy()
	{
		if (IsIndestructible || IsDestroyed)
			return;
			
		Open();
			
		_isDestroyed = true;
	}
}