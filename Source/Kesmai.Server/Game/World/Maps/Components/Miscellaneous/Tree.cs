using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Kesmai.Server.Miscellaneous.WorldForge;
using Kesmai.Server.Spells;

namespace Kesmai.Server.Game;

[WorldForgeComponent("TreeComponent")]
public class Tree : TerrainComponent
{
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

	private Terrain _terrain;
	private GrowthTimer _growthTimer;

	private TreeStages _stages;
		
	private int _currentStage;

	private bool _canGrow;
	private bool _isDecayed;
		
	public bool IsDense => (!_isDecayed && _currentStage >= (_stages.Count - 1));
		
	public bool AllowHide => IsDense;
		
	/// <summary>
	/// Initializes a new instance of the <see cref="Tree"/> class.
	/// </summary>
	public Tree(XElement element) : base(element)
	{
		_stages = new TreeStages();
			
		if (element.TryGetElement("tree", out var treeElement))
		{
			var tree = (int)treeElement;
				
			_stages = FindStages(tree);
			_currentStage = _stages.GetStage(tree);
		}

		if (element.TryGetElement("canGrow", out var canGrowElement))
			_canGrow = (bool)canGrowElement;
			
		if (element.TryGetElement("decayed", out var decayedElement))
			_isDecayed = (bool)decayedElement;

		UpdateGrowthTimer();
		UpdateTerrain();
	}

	/// <summary>
	/// Gets the terrain visible to the specified entity.
	/// </summary>
	public override IEnumerable<Terrain> GetTerrain(MobileEntity beholder)
	{
		yield return _terrain;
	}

	public void UpdateTerrain()
	{
		var stage = _currentStage;
		var max = _stages.Count - 1;

		if (stage < 0) stage = 0;
		if (stage > max) stage = max;

		var pair = _stages[stage];
			
		_terrain = (_isDecayed ? Terrain.Get(pair.Dead, Color) : Terrain.Get(pair.Alive, Color));
	}
		
	/// <summary>
	/// Grows this instance to the next stage.
	/// </summary>
	public void Grow()
	{
		if (_currentStage >= _stages.Count)
			return;

		_currentStage++;

		if (_isDecayed)
		{
			_currentStage = 0;
			_isDecayed = false;
		}
			
		UpdateGrowthTimer();
		UpdateTerrain();

		_parent.Delta(TileDelta.Terrain);
	}

	public void UpdateGrowthTimer()
	{
		if (_growthTimer != null && _growthTimer.Running)
			_growthTimer.Stop();
			
		_growthTimer = null;
			
		if (_canGrow && _currentStage < (_stages.Count - 1))
		{
			_growthTimer = new GrowthTimer(this);
			_growthTimer.Start();
		}
	}

	public void Decay()
	{
		_isDecayed = true;

		UpdateGrowthTimer();
		UpdateTerrain();
			
		_parent.Delta(TileDelta.Terrain);
	}

	protected override void OnDispose(bool disposing)
	{
		if (_growthTimer != null && _growthTimer.Running)
			_growthTimer.Stop();
			
		_growthTimer = null;
			
		base.OnDispose(disposing);
	}

	private class GrowthTimer : Timer
	{
		private Tree _tree;

		/// <summary>
		/// Initializes a new instance of the <see cref="WaterTimer"/> class.
		/// </summary>
		public GrowthTimer(Tree tree) : base(TimeSpan.FromHours(1.0))  // TODO: Scale for facet time?
		{
			_tree = tree;
		}

		/// <summary>
		/// Called when this timer has been triggered.
		/// </summary>
		protected override void OnExecute()
		{
			if (_tree != null)
				_tree.Grow();
		}
	}
}