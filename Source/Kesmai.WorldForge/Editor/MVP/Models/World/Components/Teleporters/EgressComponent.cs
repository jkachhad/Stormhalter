using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using CommonServiceLocator;
using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models
{
	public class EgressComponent : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields

		private int _egress;
		private int _destinationSegment;

		#endregion

		#region Properties and Events

		[Browsable(true)]
		public int Egress
		{
			get => _egress;
			set => _egress = value;
		}

		[Browsable(true)]
		public int DestinationSegment
		{
			get => _destinationSegment;
			set => _destinationSegment = value;
		}

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="EgressComponent"/> class.
		/// </summary>
		public EgressComponent(int egressId, int segment)
		{
			_egress = egressId;
			_destinationSegment = segment;
		}
		
		public EgressComponent(XElement element) : base(element)
		{
			_egress = (int)element.Element("egress");

			if (element.TryGetElement("destinationSegment", out var destinationElement))
				_destinationSegment = (int)destinationElement;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override IEnumerable<ComponentRender> GetTerrain()
		{
			var terrainManager = ServiceLocator.Current.GetInstance<TerrainManager>();

			if (terrainManager.TryGetValue(_egress, out Terrain terrain))
			{
				yield return new ComponentRender(terrain,
					((_destinationSegment > 0) ? Color : Microsoft.Xna.Framework.Color.Red));
			}
		}
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("egress", _egress));

			if (_destinationSegment != 0)
				element.Add(new XElement("destinationSegment", _destinationSegment));

			return element;
		}
		
		public override TerrainComponent Clone()
		{
			return new EgressComponent(GetXElement());
		}

		#endregion
	}
}
