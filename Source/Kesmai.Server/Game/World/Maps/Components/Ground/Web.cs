using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("Web")]
public class Web : Static, IHandlePathing
{
	internal new class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Web> _cache = new Dictionary<int, Web>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var allowDispel = element.GetBool("allowDispel", false);

			return Get(color, TimeSpan.Zero, allowDispel);
		}

		public Web Get(Color color, TimeSpan duration, bool allowDispel)
		{
			var hash = CalculateHash(color, duration, allowDispel);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Web(color, duration, allowDispel)));

			return component;
		}

		private static int CalculateHash(Color color, TimeSpan duration, bool allowDispel)
		{
			return HashCode.Combine(color, duration, allowDispel);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Web"/> that has been cached.
	/// </summary>
	public new static Web Construct(Color color, TimeSpan duration, bool allowDispel = false)
	{
		if (TryGetCache(typeof(Web), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, duration, allowDispel);

		return new Web(color, duration, allowDispel);
	}
	
	private static readonly Dictionary<SegmentTile, Timer> _dispelTimers = new Dictionary<SegmentTile, Timer>();

	[ServerConfigure]
	public static void Configure()
	{
		EventSink.ServerStopped += () =>
		{
			foreach (var (_, timer) in _dispelTimers) 
				timer.Stop();
			
			_dispelTimers.Clear();
		};
	}
	
	private static void StartDispelTimer(SegmentTile parent, Web component, TimeSpan duration)
	{
		if (_dispelTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_dispelTimers[parent] = Timer.DelayCall(duration, () => component.Dispel(parent));
	}

	private static void StopDispelTimer(SegmentTile parent)
	{
		if (_dispelTimers.TryGetValue(parent, out var timer))
			timer.Stop();
		
		_dispelTimers.Remove(parent);
	}
	
	private readonly TimeSpan _duration;
	private readonly bool _allowDispel;
	
	/// <inheritdoc />
	public int PathingPriority { get; } = 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="Web"/> class.
	/// </summary>
	private Web(Color color, TimeSpan duration, bool allowDispel) : base(color, 131)
	{
		_duration = duration;
		_allowDispel = allowDispel;
	}
	
	/// <inheritdoc />
	public override void Initialize(SegmentTile parent)
	{
		base.Initialize(parent);
			
		if (parent is null)
			return;
		
		parent.GetComponents<Web>().Where(t => t != this).ToList()
			.ForEach(t => t.Dispel(parent));

		if (_allowDispel && _duration > TimeSpan.Zero)
			StartDispelTimer(parent, this, _duration);
	}

	/// <summary>
	/// Determines whether the specified entity can path over this component.
	/// </summary>
	public virtual bool AllowMovementPath(SegmentTile parent, MobileEntity entity = default(MobileEntity))
	{
		return true;
	}
		
	/// <inheritdoc />
	public virtual bool AllowSpellPath(SegmentTile parent, MobileEntity entity = default(MobileEntity), Spell spell = default(Spell))
	{
		return true;
	}

	/// <summary>
	/// Handles pathing requests over this terrain.
	/// </summary>
	public void HandleMovementPath(SegmentTile parent, PathingRequestEventArgs args)
	{
		args.Result = PathingResult.Allowed;

		if (args.Entity is PlayerEntity player)
		{
			var willpower = player.Stats[EntityStat.Willpower].Value;
			var escapeChance = (player.Level / 3);

			if (willpower > 13)
				escapeChance += (willpower - 13);

			if (Utility.Random(1, 20) > escapeChance)
			{
				args.Entity.SendLocalizedMessage(6100051); /* You are caught in the web. */
				args.Result = PathingResult.Interrupted;
			}
		}
		else if (args.Entity is CreatureEntity creature && !creature.HasImmunity(CreatureImmunity.Web))
		{
			args.Result = PathingResult.Interrupted;
		}
	}

	public void Burn(SegmentTile parent)
	{
		if (_allowDispel)
			Dispel(parent);
	}
		
	public void Dispel(SegmentTile parent)
	{
		if (!_allowDispel)
			return;

		StopDispelTimer(parent);

		if (parent != null)
			parent.Remove(this);
	}
	
	protected override void OnDispose(SegmentTile parent, bool disposing)
	{
		base.OnDispose(parent, disposing);

		StopDispelTimer(parent);
	}
}