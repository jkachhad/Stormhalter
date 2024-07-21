using System.Xml;
using System.Reflection.Metadata;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;
using System.Threading;

namespace Kesmai.Server.Game;

[WorldForgeComponent("MagmaComponent")]
public class Magma : Floor, IHandlePathing
{
	private int _baseDamage;
	private Timer _internalTimer;
	private static Dictionary<MobileEntity, DateTime> _entities = new Dictionary<MobileEntity, DateTime>();	
	private static List<KeyValuePair<MobileEntity, DateTime>> _entitiesToAdd { get; } = new List<KeyValuePair<MobileEntity, DateTime>>();
	private static List<MobileEntity> _entitiesToRemove { get; } = new List<MobileEntity>();

	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Magma> _cache = new Dictionary<int, Magma>();

		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var groundId = element.GetInt("ground", 0);
			var movementCost = element.GetInt("movementCost", 3);

			return Get(color, groundId, movementCost);
		}

		public Magma Get(Color color, int MagmaId, int movementCost, int baseDamage)
		{
			var hash = CalculateHash(color, MagmaId, movementCost, baseDamage);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Magma(color, MagmaId, movementCost, baseDamage)));

			return component;
		}

		private static int CalculateHash(Color color, int MagmaId, int movementCost, int baseDamage)
		{
			return HashCode.Combine(color, MagmaId, movementCost, baseDamage);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Magma"/> that has been cached.
	/// </summary>
	public new static Magma Construct(Color color, int groundId, int movementCost, int baseDamage)
	{
		if (TryGetCache(typeof(Magma), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, groundId, movementCost, baseDamage);

		return new Magma(color, groundId, movementCost, baseDamage);
	}
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Magma"/> class.
	/// </summary>
	private Magma(Color color, int MagmaId, int movementCost, int baseDamage) : base(color, MagmaId, movementCost)
	{
		_baseDamage = baseDamage;
	}

	/// <inheritdoc />
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return true;
	}

	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

    public override void OnEnter(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{	
		if (!entity.IsAlive)
			return;

		if (!_entities.ContainsKey(entity))
			_entitiesToAdd.Add(entity, DateTime.Now);
        
        if (!region.IsInactive && !_internalTimer.Running)
        	StartTimer();	

	}

	/// <summary>
	/// Called when a mobile entity steps off this component.
	/// </summary>
	public override void OnLeave(SegmentTile parent, MobileEntity entity, bool isTeleport)
	{
		if (_entities.ContainsKey(entity))
			_entitiesToRemove.Add(entity);
		
		if (_entities.Count == 0)
			StopTimer();
	}

    private void StartTimer()
	{
		if (_timer != null)
			_timer.Stop();

		_internalTimer = Timer.DelayCall(poison.Delay, entity.Facet.TimeSpan.FromRounds(1), OnTick);
	}
	private void StopTimer()
	{
		if (_timer != null)
		{
			_internalTimer.Stop();
			_internalTimer = null;
		}
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;

		if (args.Entity is PlayerEntity player)
		{
			var balance = player.Stats[EntityStat.DexterityAdds].Value + 5;

			if (Utility.Random(1, 20) > balance)
			{
				args.Entity.SendMessage(Color.Yellow,"You lose your balance and fall in the Magma"); /* You lose your balance when you step onto the lava */
				args.Entity.Health -= (args.Entity.MaxHealth / 10);
                args.Result = PathingResult.Interrupted;
			}
		}
	}

	private void OnTick()
	{
		var tempKeysToRemove = new List<MobileEntity>(_entitiesToRemove);
		var tempKeysToAdd = new List<MobileEntity>(_entitiesToAdd);

		var damage = _baseDamage;
		var damageString = "Your boots and feet begin to burn as you sink slowly into the lava.";

		_entitiesToRemove.Clear();
		_entitiesToAdd.Clear();

		foreach (var entity in _entities)
		{
			if (entity.Key is null || entity.Value is null)
				continue;
			
			if (!entity.IsAlive)
			{
				// Not sure how to implement corpse burning here... other then strip and teleport?
				tempKeysToRemove.Add(entity);
				continue;
			}	

			TimeSpan elapsedTime = DateTime.Now - entity.Value; 

			if (elapsedTime.TotalSeconds > 5 && elapsedTime.TotalSeconds <= 10) 
			{
				damage *= 2;
				damageString = "As you sink deeper into the lava, the heat becomes unbearable.";
			}

			if (elapsedTime.TotalSeconds > 10 && elapsedTime.TotalSeconds <= 20) 
			{
				damage *= 5;
				damgageString = "The lava is now up to your waist and you can feel your skin melting.";
			}

			if (entity != null && entity.IsAlive)
			{
				entity.ApplyDamage(null, _baseDamage);
				if (entity is PlayerEntity)
					entity.SendMessage(Color.Yellow, damageString); /* You are standing in the Magma and take damage. */
			}

			if (elapsedTime.TotalSeconds > 20)
			{
				entity.Kill();
				tempKeysToRemove.Add(entity);
			}


		}

		foreach (var entity in tempKeysToRemove)
			_entities.Remove(entity);

		foreach (var entity in tempKeysToAdd)
			_entities.Add(entity, DateTime.Now);
	}
}