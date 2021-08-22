using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CommonServiceLocator;
using DigitalRune.Game.UI.Controls;
using DigitalRune.Mathematics.Algebra;
using Kesmai.WorldForge.Editor;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Kesmai.WorldForge.Models
{
	public abstract class TeleportComponent : TerrainComponent
	{
		#region Static

		#endregion

		#region Fields
		
		private int _destinationX;
		private int _destinationY;
		private int _destinationRegion;

		#endregion

		#region Properties and Events
		
		/// <summary>
		/// Gets the destination x-coordinate.
		/// </summary>
		[Browsable(true)]
		public int DestinationX
		{
			get { return _destinationX; }
			set { _destinationX = value; }
		}

		/// <summary>
		/// Gets the destination y-coordinate.
		/// </summary>
		[Browsable(true)]
		public int DestinationY
		{
			get { return _destinationY; }
			set { _destinationY = value; }
		}

		/// <summary>
		/// Gets the destination region.
		/// </summary>
		[Browsable(true)]
		public int DestinationRegion
		{
			get { return _destinationRegion; }
			set { _destinationRegion = value; }
		}

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="TeleportComponent"/> class.
		/// </summary>
		public TeleportComponent(int x, int y, int region)
		{
			_destinationX = x;
			_destinationY = y;
			_destinationRegion = region;
		}
		
		public TeleportComponent(XElement element) : base(element)
		{
			var xElement = element.Element("destinationX");

			if (xElement != null)
				_destinationX = (int)xElement;

			var yElement = element.Element("destinationY");

			if (yElement != null)
				_destinationY = (int)yElement;

			var regionElement = element.Element("destinationRegion");

			if (regionElement != null)
				_destinationRegion = (int)regionElement;
		}

		#endregion

		#region Methods
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			if (_destinationX != 0)
				element.Add(new XElement("destinationX", _destinationX));

			if (_destinationY != 0)
				element.Add(new XElement("destinationY", _destinationY));

			if (_destinationRegion != 0)
				element.Add(new XElement("destinationRegion", _destinationRegion));

			return element;
		}

		protected bool IsValid()
		{
			var segmentRequest = WeakReferenceMessenger.Default.Send<GetActiveSegmentRequestMessage>();
			var segment = segmentRequest.Response;

			if (segment != null)
			{
				var region = segment.GetRegion(_destinationRegion);

				if (region != null)
				{
					var tile = region.GetTile(_destinationX, _destinationY);

					if (tile != null)
						return true;
				}
			}

			return false;
		}
		

		#endregion
	}
}
