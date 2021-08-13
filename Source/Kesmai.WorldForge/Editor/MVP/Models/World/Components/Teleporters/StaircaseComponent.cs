using System;
using System.IO;
using System.Xml.Linq;

using Kesmai.WorldForge.Models;

namespace Kesmai.WorldForge.Models
{
	public class StaircaseComponent : ActiveTeleporter
	{
		#region Static

		#endregion

		#region Fields
		

		#endregion

		#region Properties and Events
		
		public bool Descends { get; set; }

		#endregion

		#region Constructors and Cleanup

		/// <summary>
		/// Initializes a new instance of the <see cref="StaircaseComponent"/> class.
		/// </summary>
		public StaircaseComponent(int staircaseId, int x, int y, int region, bool descends = false) : base(staircaseId, x, y, region)
		{
			Descends = descends;
		}
		
		public StaircaseComponent(XElement element) : base(element)
		{
			Descends = (bool)element.Element("descends");
		}


		#endregion

		#region Methods
		
		public override XElement GetXElement()
		{
			var element = base.GetXElement();

			element.Add(new XElement("descends", Descends));

			return element;
		}

		public override TerrainComponent Clone()
		{
			return new StaircaseComponent(GetXElement());
		}

		#endregion
	}
}
