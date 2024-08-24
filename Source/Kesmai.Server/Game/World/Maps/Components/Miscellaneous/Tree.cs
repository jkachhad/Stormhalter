using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("TreeComponent")]
public class Tree : TerrainComponent
{
	internal class Cache : IComponentCache
	{
		private static readonly Dictionary<int, Tree> _cache = new Dictionary<int, Tree>();
	
		public TerrainComponent Get(XElement element)
		{
			var color = element.GetColor("color", Color.White);
			var treeId = element.GetInt("tree", 0);
			var canGrow = element.GetBool("canGrow", false);
			var isDecayed = element.GetBool("decayed", false);

			return Get(color, treeId, canGrow, isDecayed);
		}

		public Tree Get(Color color, int treeId, bool canGrow, bool isDecayed)
		{
			var hash = CalculateHash(color, treeId, canGrow, isDecayed);

			if (!_cache.TryGetValue(hash, out var component))
				_cache.Add(hash, (component = new Tree(color, treeId, canGrow, isDecayed)));

			return component;
		}

		private static int CalculateHash(Color color, int treeId, bool canGrow, bool isDecayed)
		{
			return HashCode.Combine(color, treeId, canGrow, isDecayed);
		}
	}
	
	/// <summary>
	/// Gets an instance of <see cref="Tree"/> that has been cached.
	/// </summary>
	public static Tree Construct(Color color, int treeId, bool canGrow, bool decayed)
	{
		if (TryGetCache(typeof(Tree), out var cache) && cache is Cache componentCache)
			return componentCache.Get(color, treeId, canGrow, decayed);
		
		return new Tree(color, treeId, canGrow, decayed);
	}
	
	private static readonly Dictionary<SegmentTile, Timer> _growthTimer = new Dictionary<SegmentTile, Timer>();

	[ServerConfigure]
	public static void Configure()
	{
		EventSink.ServerStopped += () =>
		{
			foreach (var (_, timer) in _growthTimer) 
				timer.Stop();
			
			_growthTimer.Clear();
		};
	}
	
	private static void StartGrowthTimer(SegmentTile parent, Tree component)
	{
		if (_growthTimer.TryGetValue(parent, out var timer))
			timer.Stop();
		
		timer = _growthTimer[parent] = new GrowthTimer(parent, component);
		timer.Start();
	}

	private static void StopGrowthTimer(SegmentTile parent)
	{
		if (_growthTimer.TryGetValue(parent, out var timer))
			timer.Stop();

		_growthTimer.Remove(parent);
	}
	
	private class TreeStagePair
	{
		public int Alive { get; set; }
		public int Dead { get; set; }

		public TreeStagePair(int alive, int dead)
		{
			Alive = alive;
			Dead = dead;
		}
	}
		
	private class TreeStages : List<TreeStagePair>
	{
		public TreeStagePair GetPair(int treeId)
		{
			return this.FirstOrDefault(p => p.Alive == treeId || p.Dead == treeId);
		}
			
		public int GetStage(int treeId)
		{
			var pair = GetPair(treeId);

			if (pair != default(TreeStagePair))
				return IndexOf(pair);

			return 1;
		}
	}
		
	private static List<TreeStages> _treeStages = new List<TreeStages>()
	{
		new TreeStages() { new TreeStagePair(100, 103), new TreeStagePair(99, 102), new TreeStagePair(98, 101) },
		new TreeStages() { new TreeStagePair(201, 103), new TreeStagePair(200, 102), new TreeStagePair(199, 101) },
		new TreeStages() { new TreeStagePair(204, 103), new TreeStagePair(203, 102), new TreeStagePair(202, 101) },
		new TreeStages() { new TreeStagePair(247, 101), },
		new TreeStages() { new TreeStagePair(370, 372), },
		new TreeStages() { new TreeStagePair(403, 402), },
		new TreeStages() { new TreeStagePair(625, 103), },
		new TreeStages() { new TreeStagePair(901, 103), },
		new TreeStages() { new TreeStagePair(893, 897), },
		new TreeStages() { new TreeStagePair(894, 897), },
		new TreeStages() { new TreeStagePair(895, 897), },
		new TreeStages() { new TreeStagePair(896, 897), },
		new TreeStages() { new TreeStagePair(898, 103), },
		new TreeStages() { new TreeStagePair(899, 103), },
		new TreeStages() { new TreeStagePair(900, 103), },
		new TreeStages() { new TreeStagePair(901, 103), },
		new TreeStages() { new TreeStagePair(902, 103), },
		new TreeStages() { new TreeStagePair(905, 103), },
		new TreeStages() { new TreeStagePair(906, 103), },
        new TreeStages() { new TreeStagePair(1004, 103), },

        new TreeStages() { new TreeStagePair(624, 624), },
		new TreeStages() { new TreeStagePair(625, 625), },
		new TreeStages() { new TreeStagePair(626, 626), },
		new TreeStages() { new TreeStagePair(627, 627), },
		new TreeStages() { new TreeStagePair(1112, 1113), },
		new TreeStages() { new TreeStagePair(1373, 1378), },
		new TreeStages() { new TreeStagePair(1374, 1378), },
		new TreeStages() { new TreeStagePair(1375, 1378), },
		new TreeStages() { new TreeStagePair(1376, 1378), },
		new TreeStages() { new TreeStagePair(1377, 1378), },
	};
		
	private static TreeStages FindStages(int treeId)
	{
		foreach (var stages in _treeStages)
		{
			if (stages.Any(stage => stage.Alive == treeId || stage.Dead == treeId))
				return stages;
		}

		return default(TreeStages);
	}

	private readonly Terrain _terrain;

	private readonly TreeStages _stages;
		
	private readonly int _currentStage;

	private readonly bool _canGrow;
	private readonly bool _isDecayed;
		
	/// <summary>
	/// Gets a value indicating if this instance is dense.
	/// </summary>
	public bool IsDense => (!_isDecayed && _currentStage >= (_stages.Count - 1));
		
	/// <summary>
	/// Gets a value indicating if a thief can hide near this instance.
	/// </summary>
	public bool AllowHide => IsDense;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tree"/> class.
	/// </summary>
	private Tree(Color color, int treeId, bool canGrow, bool decayed) : base(color)
	{
		_stages = new TreeStages();
		
		if (treeId > 0)
		{
			_stages = FindStages(treeId);
			_currentStage = _stages.GetStage(treeId);
		}

		_canGrow = canGrow;
		_isDecayed = decayed;
		
		var stage = _currentStage;
		var max = _stages.Count - 1;

		if (stage < 0) stage = 0;
		if (stage > max) stage = max;

		var pair = _stages[stage];
			
		_terrain = (_isDecayed ? Terrain.Get(pair.Dead, Color) : Terrain.Get(pair.Alive, Color));
	}

	/// <inheritdoc />
	public override void Initialize(SegmentTile parent)
	{
		base.Initialize(parent);

		if (parent is null)
			return;
		
		if (_canGrow && (_isDecayed || _currentStage < _stages.Count - 1))
			StartGrowthTimer(parent, this);
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(SegmentTile parent, MobileEntity beholder)
	{
		yield return _terrain;
	}

	/// <summary>
	/// Grows this instance to the next stage.
	/// </summary>
	/// <param name="parent"></param>
	public void Grow(SegmentTile parent)
	{
		StopGrowthTimer(parent);
		
		if ((_currentStage >= _stages.Count))
			return;
		
		var stage = _currentStage + 1;
		var max = _stages.Count - 1;

		if (stage < 0) stage = 0;
		if (stage > max) stage = max;

		var pair = _stages[stage];

		var nextTerrain = Terrain.Get(pair.Alive, Color);
			
		var currentState = this;
		var updatedState = Construct(_color, nextTerrain.ID, _canGrow, false);
		
		if (currentState != updatedState)
		{
			parent.Remove(currentState);
			parent.Add(updatedState);
		}
		
		parent.Delta(TileDelta.Terrain);
	}
	
	public void Decay(SegmentTile parent)
	{
		StopGrowthTimer(parent);
		
		var stage = _currentStage;
		var max = _stages.Count - 1;

		if (stage < 0) stage = 0;
		if (stage > max) stage = max;

		var pair = _stages[stage];

		var nextTerrain = Terrain.Get(pair.Dead, _color);
		
		var currentState = this;
		var updatedState = Construct(_color, nextTerrain.ID, _canGrow, true);

		if (currentState != updatedState)
		{
			parent.Remove(currentState);
			parent.Add(updatedState);
		}
		
		parent.Delta(TileDelta.Terrain);
	}

	protected override void OnDispose(SegmentTile parent, bool disposing)
	{
		base.OnDispose(parent, disposing);
		
		StopGrowthTimer(parent);
	}

	private class GrowthTimer : Timer
	{
		private SegmentTile _segmentTile;
		private Tree _tree;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaterTimer"/> class.
		/// </summary>
		public GrowthTimer(SegmentTile segmentTile, Tree tree) : base(segmentTile.Facet.TimeSpan.FromMinutes(1.0))
		{
			_segmentTile = segmentTile;
			_tree = tree;
		}

		/// <summary>
		/// Called when this timer has been triggered.
		/// </summary>
		protected override void OnExecute()
		{
			if (_tree != null)
				_tree.Grow(_segmentTile);
		}
	}
}