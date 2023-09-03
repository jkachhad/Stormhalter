using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;
using Kesmai.Server.Items;
using Kesmai.Server.Miscellaneous.WorldForge;

namespace Kesmai.Server.Game;

[WorldForgeComponent("FloorComponent")]
public class Floor : TerrainComponent, IHandleInteraction, IHandleMovement, IHandleItems
{
	private Terrain _ground;
	private int _movementCost;

	private int _corpses;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Floor"/> class.
	/// </summary>
	public Floor(XElement element) : base(element)
	{
		if (element.TryGetElement("ground", out var groundElement))
			_ground = Terrain.Get((int)groundElement, Color);

		if (element.TryGetElement("movementCost", out var movementCostElement))
			_movementCost = (int)movementCostElement;
		else
			_movementCost = 1;
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		if (_ground != null)
			yield return _ground;
	}

	/// <summary>
	/// Handles interaction from the specified entity.
	/// </summary>
	public bool HandleInteraction(MobileEntity entity, ActionType action)
	{
		if (action != ActionType.Look)
			return false;

		var location = _parent.Location;

		var distance = entity.GetDistanceToMax(location);

		if (distance > 1)
			entity.SendLocalizedMessage(Color.Red, 6300103); /* You are unable to look from here. */
		else
			entity.LookAt(_parent);

		return true;
	}

	/// <summary>
	/// Gets the movement cost for walking off this terrain.
	/// </summary>
	public virtual int GetMovementCost(MobileEntity entity) => _movementCost;

	/// <summary>
	/// Called when a mobile entity steps on this component.
	/// </summary>
	public virtual void OnEnter(MobileEntity entity)
	{
		/* Play a sound effect when walking over corpses. */
		if (_corpses > 0 && entity is PlayerEntity)
		{
			if (Utility.Random(1, 20) <= _corpses)
			{
				var sound = 3001;

				if (_corpses > 3)
					sound = Utility.RandomBool() ? 3002 : 3003;

				entity.PlaySound(sound);
			}
		}
	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public virtual void OnLeave(MobileEntity entity)
	{
	}

	/// <inheritdoc />
	public void OnItemAdded(ItemEntity item)
	{
		if (item is Corpse)
			_corpses++;
	}

	/// <inheritdoc />
	public void OnItemRemoved(ItemEntity item)
	{
		if (item is Corpse)
			_corpses--;

		if (_corpses < 0)
			_corpses = 0;
	}
}