using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models
{
	public class TreeComponent : TerrainComponent
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
		
		#region Static

		private static List<TreeStages> _treeStages = new List<TreeStages>()
		{
			new TreeStages() { new TreeStagePair(100, 103), new TreeStagePair(99, 102), new TreeStagePair(98, 101) },
			new TreeStages() { new TreeStagePair(201, 103), new TreeStagePair(200, 102), new TreeStagePair(199, 101) },
			new TreeStages() { new TreeStagePair(204, 103), new TreeStagePair(203, 102), new TreeStagePair(202, 101) },
			new TreeStages() { new TreeStagePair(247, 101), },
			new TreeStages() { new TreeStagePair(370, 372), },
			new TreeStages() { new TreeStagePair(403, 402), },
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
		
		#endregion

		#region Fields

		private int _tree;

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets or sets the tree identifier.
		/// </summary>
		[Browsable(true)]
		public int Tree
		{
			get { return _tree; }
			set { _tree = value; }
		}

		[Browsable(true)]
		public bool CanGrow { get; set; } = true;

		[Browsable(true)]
		public bool IsDecayed { get; set; } = false;

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="Tree"/> class.
		/// </summary>
		public TreeComponent(int tree)
		{
			_tree = tree;
		}
		
		public TreeComponent(XElement element) : base(element)
		{
			_tree = (int)element.Element("tree");
			CanGrow = (bool)element.Element("canGrow");

			var decayedElement = element.Element("decayed");

			if (decayedElement != null)
				IsDecayed = (bool)decayedElement;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();
			Terrain terrain;

			var stages = FindStages(_tree);
			if (stages is not null)
			{
				var pair = stages.GetPair(_tree);

				terrainManager.TryGetValue((IsDecayed ? pair.Dead : pair.Alive), out terrain);
			}
			else
			{
				terrainManager.TryGetValue((IsDecayed ? 101 : _tree), out terrain); // default to a dead kesmai tree if a pairing is not found
			}

			if (terrain is not null)
				yield return new ComponentRender(terrain, Color);

		}

		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("tree", _tree));
			element.Add(new XElement("canGrow", CanGrow));

			if (IsDecayed)
				element.Add(new XElement("decayed", IsDecayed));
			
			return element;
		}
		
		public override TerrainComponent Clone()
		{
			return new TreeComponent(GetXElement());
		}

		#endregion
	}
}
