using CommonServiceLocator;
using Kesmai.WorldForge.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models
{
	public class ObstructionComponent : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields

		private int _obstruction;
		private bool _blockVision;

		#endregion

		#region Properties and Events

		/// <summary>
		/// Gets the obstruction terrain.
		/// </summary>
		[Browsable(true)]
		public int Obstruction
		{
			get { return _obstruction; }
			set { _obstruction = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance blocks line-of-sight.
		/// </summary>
		[Browsable(true)]
		public bool BlockVision
		{
			get { return _blockVision; }
			set { _blockVision = value; }
		}

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="ObstructionComponent"/> class.
		/// </summary>
		public ObstructionComponent(int obstructionId, bool blockVision)
		{
			_obstruction = obstructionId;
			_blockVision = blockVision;
		}
		
		public ObstructionComponent(XElement element) : base(element)
		{
			_obstruction = (int)element.Element("obstruction");

			var blockVisionElement = element.Element("blockVision");

			if (blockVisionElement != null)
				_blockVision = (bool)blockVisionElement;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			if (terrainManager.TryGetValue(_obstruction, out Terrain terrain))
				yield return new ComponentRender(terrain, Color);
		}

		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("obstruction", _obstruction));

			if (_blockVision)
				element.Add(new XElement("blockVision", _blockVision));

			return element;
		}

		public override TerrainComponent Clone()
		{
			return new ObstructionComponent(GetXElement());
		}

		#endregion
	}
}
